namespace zoft.MauiExtensions.Controls
{
    /// <summary>
    /// Provides event data for the <see cref="AutoCompleteEntry.QuerySubmitted"/> event.
    /// </summary>
    public sealed class AutoCompleteEntryQuerySubmittedEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AutoCompleteEntryQuerySubmittedEventArgs"/> class.
        /// </summary>
        /// <param name="queryText"></param>
        /// <param name="chosenSuggestion"></param>
        internal AutoCompleteEntryQuerySubmittedEventArgs(string queryText, object chosenSuggestion)
        {
            QueryText = queryText;
            ChosenSuggestion = chosenSuggestion;
        }

        /// <summary>
        /// Gets the suggested result that the use chose.
        /// </summary>
        /// <value>The suggested result that the use chose.</value>
        public object ChosenSuggestion { get; }

        /// <summary>
        /// The query text of the current search.
        /// </summary>
        /// <value>Gets the query text of the current search.</value>
        public string QueryText { get; }
    }
}
