namespace FootballManager.Plugin.Data;

// What of a binding's value to keep once its request is gone, to read further from it.
internal enum PropertyCapture
{
    None,

    // The value itself, e.g. a record further requests read properties of.
    Value,

    // The items of a list-valued path, e.g. the players a request then runs once for each.
    Items
}
