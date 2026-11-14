using FluentResults;
using FootballManager.Application.Exceptions;
using FootballManager.Application.Interfaces;
using FootballManager.Application.ResultErrors;
using FootballManager.Domain.Training;

namespace FootballManager.Application.Services;

public interface ITrainingService
{
    /// <summary>This week's and next week's schedule, the session types, and each player's own training.</summary>
    Task<Result<TrainingOverview>> GetTrainingAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Plans the changes against what the game offers each player and each session, and sends them only when
    /// <paramref name="apply"/> is set, reading each back afterwards.
    /// </summary>
    Task<Result<TrainingChanges>> SetTrainingAsync(TrainingEdits edits, bool apply, CancellationToken cancellationToken = default);
}

// A player's training changes through the dropdowns of his individual training screen, a session through its card in
// the training calendar. Both take only what the game offers there, which the plan checks first; nothing is written
// without apply. Once sent, every change is read back from the game.
public class TrainingService(ITrainingGameClient gameClient) : ITrainingService
{
    public const int MaxPlayers = 50;

    // Three a day across the two weeks the calendar reads.
    public const int MaxSessions = 42;

    public const int SessionsPerDay = 3;

    // The game's ids for no additional focus, and for leaving the intensity to the game. Intensities run up to double
    // (2); the ids above are rest periods, which the game then shows as the days left rather than the period picked.
    public const long NoFocus = -1;
    public const long AutomaticIntensity = -1;
    public const long HighestIntensity = 2;

    // The game shows a change a frame or more after it's sent.
    private const int ReadBacks = 3;
    private static readonly TimeSpan ReadBackPause = TimeSpan.FromMilliseconds(300);

    public async Task<Result<TrainingOverview>> GetTrainingAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return Result.Ok(await gameClient.GetTrainingAsync(cancellationToken));
        }
        catch (GameUnavailableException exception)
        {
            return Result.Fail<TrainingOverview>(new GameUnavailableError(exception.Message));
        }
    }

    public async Task<Result<TrainingChanges>> SetTrainingAsync(TrainingEdits edits, bool apply, CancellationToken cancellationToken = default)
    {
        var errors = Check(edits);

        if (errors.Count > 0)
        {
            return Result.Fail<TrainingChanges>(errors);
        }

        try
        {
            var training = await gameClient.GetTrainingAsync(cancellationToken);
            var changes = new TrainingChanges { Applied = apply };
            errors = Plan(edits, training, changes);

            if (errors.Count > 0)
            {
                return Result.Fail<TrainingChanges>(errors);
            }

            if (apply)
            {
                await SendAsync(changes, cancellationToken);
                await ReadBackAsync(changes, cancellationToken);
            }

            return Result.Ok(changes);
        }
        catch (GameUnavailableException exception)
        {
            return Result.Fail<TrainingChanges>(new GameUnavailableError(exception.Message));
        }
    }

    // What can be told without the game.
    internal static List<IError> Check(TrainingEdits edits)
    {
        var errors = new List<IError>();

        if (edits.Players.Count == 0 && edits.Sessions.Count == 0)
        {
            errors.Add(new ValidationError("edits", ["Give at least one player or session to change."]));
        }

        if (edits.Players.Count > MaxPlayers)
        {
            errors.Add(new ValidationError("players", [$"At most {MaxPlayers} players at a time."]));
        }

        if (edits.Sessions.Count > MaxSessions)
        {
            errors.Add(new ValidationError("sessions", [$"At most {MaxSessions} sessions at a time."]));
        }

        var players = edits.Players.GroupBy(player => player.PlayerId).Where(group => group.Count() > 1).Select(group => group.Key).ToList();

        if (players.Count > 0)
        {
            errors.Add(new ValidationError("players", [$"Each player once: {string.Join(", ", players)} come more than once."]));
        }

        foreach (var player in edits.Players.Where(player => player.Focus is null && player.Intensity is null))
        {
            errors.Add(new ValidationError("players", [$"Player {player.PlayerId}: give a focus, an intensity or both."]));
        }

        foreach (var session in edits.Sessions.Where(session => session.Slot is < 0 or >= SessionsPerDay))
        {
            errors.Add(new ValidationError("sessions", [$"{session.Date}: a day's sessions are slots 0 to {SessionsPerDay - 1}, not {session.Slot}."]));
        }

        var sessions = edits.Sessions.GroupBy(session => (session.Date, session.Slot)).Where(group => group.Count() > 1).Select(group => group.Key).ToList();

        if (sessions.Count > 0)
        {
            errors.Add(new ValidationError("sessions",
                [$"Each session once: {string.Join(", ", sessions.Select(session => $"{session.Date} slot {session.Slot}"))} come more than once."]));
        }

        return errors;
    }

    // Each change against what the game offers now; a change to what's already set is left out with a note.
    internal static List<IError> Plan(TrainingEdits edits, TrainingOverview training, TrainingChanges changes)
    {
        var errors = new List<IError>();
        var focuses = training.FocusOptions.ToDictionary(option => option.Id);
        var intensities = training.IntensityOptions.ToDictionary(option => option.Id);

        foreach (var edit in edits.Players)
        {
            var player = training.Players.FirstOrDefault(p => p.PlayerId == edit.PlayerId);

            if (player is null)
            {
                errors.Add(new ValidationError("players", [$"No player of the training squad has id {edit.PlayerId} (see get_training)."]));
                continue;
            }

            var change = new PlayerTrainingChange { PlayerId = player.PlayerId, Name = player.Name };

            if (edit.Focus is { } focus)
            {
                if (!player.CanEditFocus)
                {
                    errors.Add(new ValidationError("players", [$"{player.Name}'s focus is set by the staff; the user can take his training back in the game."]));
                }
                else if (!player.FocusChoices.Contains(focus) || !focuses.TryGetValue(focus, out var option))
                {
                    errors.Add(new ValidationError("players", [$"{player.Name} can't take focus {focus}; his choices are {string.Join(", ", player.FocusChoices)}."]));
                }
                else if (player.Focus?.Id == focus || (player.Focus is null && focus == NoFocus))
                {
                    changes.Notes.Add($"{player.Name} already has that focus ({option.Name}).");
                }
                else
                {
                    change.Focus = new OptionChange { From = player.Focus, To = option };
                }
            }

            if (edit.Intensity is { } intensity)
            {
                if (!player.CanEditIntensity)
                {
                    errors.Add(new ValidationError("players", [$"{player.Name}'s intensity is set by the medical staff while he's injured."]));
                }
                else if (!player.IntensityChoices.Contains(intensity) || !intensities.TryGetValue(intensity, out var option))
                {
                    errors.Add(new ValidationError("players", [$"{player.Name} can't take intensity {intensity}; his choices are {string.Join(", ", player.IntensityChoices)}."]));
                }
                else if (intensity == AutomaticIntensity ? player.IntensityIsAutomatic : player.Intensity?.Id == intensity && !player.IntensityIsAutomatic)
                {
                    changes.Notes.Add($"{player.Name} already trains at that intensity ({option.Name}).");
                }
                else
                {
                    change.Intensity = new OptionChange { From = player.Intensity, To = option };
                }
            }

            if (change.Focus is not null || change.Intensity is not null)
            {
                changes.Players.Add(change);
            }
        }

        var types = training.SessionTypes.SelectMany(category => category.Sessions).ToDictionary(option => option.Id);

        foreach (var edit in edits.Sessions)
        {
            var week = training.Weeks.FirstOrDefault(w => w.Days.Any(day => day.Date == edit.Date));
            var current = week?.Days.First(day => day.Date == edit.Date).Sessions.FirstOrDefault(session => session.Slot == edit.Slot);

            if (week is null || current is null)
            {
                errors.Add(new ValidationError("sessions", [$"{edit.Date} slot {edit.Slot} isn't a session of this week or the next (see get_training)."]));
                continue;
            }

            if (!current.CanEdit)
            {
                errors.Add(new ValidationError("sessions", [$"{edit.Date} slot {edit.Slot} ({current.Name}) can't be changed."]));
                continue;
            }

            if (!types.TryGetValue(edit.Session, out var option))
            {
                errors.Add(new ValidationError("sessions", [$"There's no session {edit.Session} (see sessionTypes in get_training)."]));
                continue;
            }

            if (current.Type == edit.Session)
            {
                changes.Notes.Add($"{edit.Date} slot {edit.Slot} is already {option.Name}.");
                continue;
            }

            changes.Sessions.Add(new SessionChange
            {
                Date = edit.Date,
                Slot = edit.Slot,
                Change = new OptionChange { From = new TrainingOption { Id = current.Type, Name = current.Name }, To = option }
            });
        }

        return errors;
    }

    // One change at a time; the first the game refuses or doesn't answer stops the rest, which the read back then shows.
    private async Task SendAsync(TrainingChanges changes, CancellationToken cancellationToken)
    {
        var sends = new List<(string What, Func<Task> Send)>();

        foreach (var player in changes.Players)
        {
            if (player.Focus?.To is { } focus)
            {
                sends.Add(($"{player.Name}'s focus", () => gameClient.SetFocusAsync(player.PlayerId, focus.Id, cancellationToken)));
            }

            if (player.Intensity?.To is { } intensity)
            {
                sends.Add(($"{player.Name}'s intensity", () => gameClient.SetIntensityAsync(player.PlayerId, intensity.Id, cancellationToken)));
            }
        }

        foreach (var session in changes.Sessions)
        {
            sends.Add(($"{session.Date} slot {session.Slot}", () => gameClient.SetSessionAsync(
                session.Date, session.Slot, (int)session.Change.From!.Id, (int)session.Change.To!.Id, cancellationToken)));
        }

        foreach (var (what, send) in sends)
        {
            try
            {
                await send();
            }
            catch (Exception exception) when (exception is GameRejectedException or GameUnavailableException)
            {
                changes.Notes.Add($"The game didn't take {what} ({exception.Message}); nothing after it was sent.");
                return;
            }
        }
    }

    private async Task ReadBackAsync(TrainingChanges changes, CancellationToken cancellationToken)
    {
        for (var read = 0; read < ReadBacks; read++)
        {
            if (read > 0)
            {
                await Task.Delay(ReadBackPause, cancellationToken);
            }

            TrainingOverview training;

            try
            {
                training = await gameClient.GetTrainingAsync(cancellationToken);
            }
            catch (GameUnavailableException exception)
            {
                changes.Notes.Add($"The changes couldn't be read back ({exception.Message}): check them with get_training.");
                return;
            }

            if (ReadBack(changes, training))
            {
                return;
            }
        }
    }

    // Marks each change done or not from the game's current state; true when all are done.
    internal static bool ReadBack(TrainingChanges changes, TrainingOverview training)
    {
        foreach (var change in changes.Players)
        {
            var player = training.Players.FirstOrDefault(p => p.PlayerId == change.PlayerId);

            if (change.Focus is { } focus)
            {
                focus.Done = (player?.Focus?.Id ?? NoFocus) == focus.To?.Id;
            }

            if (change.Intensity is { } intensity)
            {
                var now = player?.Intensity?.Id;
                intensity.Done = intensity.To?.Id switch
                {
                    AutomaticIntensity => player?.IntensityIsAutomatic == true,
                    > HighestIntensity => player?.RestDays > 0,
                    var to => player is { IntensityIsAutomatic: false } && now == to
                };
            }
        }

        foreach (var change in changes.Sessions)
        {
            var session = training.Weeks.SelectMany(week => week.Days).FirstOrDefault(day => day.Date == change.Date)?
                .Sessions.FirstOrDefault(s => s.Slot == change.Slot);
            change.Change.Done = session?.Type == change.Change.To?.Id;
        }

        return changes.Players.All(p => p.Focus?.Done != false && p.Intensity?.Done != false)
               && changes.Sessions.All(s => s.Change.Done == true);
    }
}
