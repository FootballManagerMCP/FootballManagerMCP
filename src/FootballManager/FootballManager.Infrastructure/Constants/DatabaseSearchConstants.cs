using System.Globalization;
using FootballManager.Domain.Database;

namespace FootballManager.Infrastructure.Constants;

// The game's search box, on the game root with the user's manager and team as contexts (without them the game ignores
// the text). Typing is a write of its text; the results are a list of SearchResult records, closest match first, each
// with the database record it stands for (RowData).
internal static class DatabaseSearchConstants
{
    public const string Search = "Search";
    public const string Text = "Name";
    public const string IsFinished = "SearchIsFinished";
    public const string Results = "Search.Results";

    // On a result.
    public const string RecordType = "DatabaseRecordType";
    public const string TypeLabel = "SearchRecordType";
    public const string Description = "Description";
    public const string Linked = "LinkedRecord.Name";
    public const string SecondaryLinked = "SecondaryLinkedRecord.Name";

    // The record it stands for, and a person's club, under a result.
    public const string Record = "RowData";
    public const string PersonClub = "RowData.Club";

    // On the record. A person's positions are only a player's: a staff member's are filler.
    public const string Id = "RowData.UniqueId";
    public const string LongName = "RowData.LongName";
    public const string IsPlayer = "RowData.IsPlayer";
    public const string Age = "RowData.Age";
    public const string Positions = "RowData.PositionCombinedString";
    public const string Job = "RowData.JobString";
    public const string Nation = "RowData.Nation.Name";
    public const string Club = "RowData.Club.Name";
    public const string ClubId = "RowData.Club.UniqueId";
    public const string ClubNation = "RowData.Club.Nation.Name";

    // The league's short name; a club's search line gives its long one.
    public const string League = "RowData.Division.Name";

    public static readonly string[] ResultProperties =
    [
        GameConstants.Name, RecordType, TypeLabel, Description, Linked, SecondaryLinked, Id, LongName, IsPlayer, Age,
        Positions, Job, Nation, Club, ClubId, ClubNation, League
    ];

    // A person is a player or staff (IsPlayer tells them apart). Other types seen: 0 an award, 7 a media outlet, 8 a
    // language.
    public const int Person = 1;

    public static readonly IReadOnlyDictionary<int, DatabaseRecordKind> Kinds = new Dictionary<int, DatabaseRecordKind>
    {
        [3] = DatabaseRecordKind.Club,
        [9] = DatabaseRecordKind.NationalTeam,
        [10] = DatabaseRecordKind.Nation,
        [25] = DatabaseRecordKind.Competition
    };

    // At most this many results are read: past a couple of hundred the game leaves some unanswered, and the close
    // matches come first.
    public const int MaxRead = 200;

    public static string Result(int position)
    {
        return $"{Results}[{position.ToString(CultureInfo.InvariantCulture)}]";
    }
}
