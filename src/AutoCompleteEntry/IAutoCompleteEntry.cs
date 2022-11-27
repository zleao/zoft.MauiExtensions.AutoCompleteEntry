using System.Collections;
using zoft.MauiExtensions.Core.Commands;

namespace zoft.MauiExtensions.Controls
{
	/// <summary>
	/// Represents a text control that makes suggestions to users as they type. The app is notified when text 
	/// has been changed by the user and is responsible for providing relevant suggestions for this control to display.
	/// </summary>
	public interface IAutoCompleteEntry : ISearchBar
	{
        /// <summary>
        /// Gets or sets the property path that is used to get the value for display in the
        /// text box portion of the <see cref="AutoCompleteEntry"/> control, when an item is selected.
        /// </summary>
        /// <value>
        /// The property path that is used to get the value for display in the text box portion
        /// of the <see cref="AutoCompleteEntry"/> control, when an item is selected.
        /// </value>
        string TextMemberPath { get; set; }

		/// <summary>
		/// Gets or sets the name or path of the property that is displayed for each data item.
		/// </summary>
		/// <value>
		/// The name or path of the property that is displayed for each the data item in
		/// the control. The default is an empty string ("").
		/// </value>
		string DisplayMemberPath { get; set; }

        /// <summary>
        /// Gets or sets a Boolean value indicating whether the drop-down portion of the <see cref="AutoCompleteEntry"/> is open.
        /// </summary>
        /// <value>A Boolean value indicating whether the drop-down portion of the <see cref="AutoCompleteEntry"/> is open.</value>
        bool IsSuggestionListOpen { get; set; }

        /// <summary>
        /// Used in conjunction with <see cref="TextMemberPath"/>, gets or sets a value indicating whether items in the view will trigger an update 
        /// of the editable text part of the <see cref="AutoCompleteEntry"/> when clicked.
        /// </summary>
        /// <value>A value indicating whether items in the view will trigger an update of the editable text part of the <see cref="AutoCompleteEntry"/> when clicked.</value>
        bool UpdateTextOnSelect { get; set; }

		/// <summary>
		/// Gets or sets the ItemsSource list with the suggestions to diplay.
		/// </summary>
		/// <value>The header object for the text box portion of this control.</value>
		IList ItemsSource { get; set; }


        /// <summary>
        /// Method used to signal the platform control, that the text changed
        /// </summary>
        /// <param name="text"></param>
        /// <param name="reason"></param>
        internal void OnTextChanged(string text, AutoCompleteEntryTextChangeReason reason);

        /// <summary>
        /// Raised after the text content of the editable control component is updated.
        /// </summary>
        event EventHandler<AutoCompleteEntryTextChangedEventArgs> TextChanged;

        /// <summary>
        /// Gets or Sets the TextChangedCommand, that is trigered everytime the text changes.
        /// The command receives as parameter the changed text.
        /// </summary>
        Microsoft.Maui.Controls.Command<string> TextChangedCommand { get; set; }


        /// <summary>
        /// Method used to signal the platform control, that a suggestion was selected.
        /// </summary>
        /// <param name="selectedItem">Item selected</param>
        internal void OnSuggestionSelected(object selectedItem);

        /// <summary>
        /// Raised before the text content of the editable control component is updated.
        /// </summary>
        event EventHandler<AutoCompleteEntrySuggestionChosenEventArgs> SuggestionChosen;

        /// <summary>
        /// Gets or Sets the SuggestionChosenCommand, that is trigered everytime an item on the ItemsSource is choosen.
        /// The command receives as parameter, the selected item.
        /// </summary>
        object SelectedSuggestion { get; set; }


        /// <summary>
        /// Event fired everytime . The reason for the change is sent in the event parameters
        /// </summary>
        /// <param name="queryText"></param>
        /// <param name="chosenSuggestion"></param>
        internal void OnQuerySubmitted(string queryText, object chosenSuggestion);

        /// <summary>
        /// Occurs when the a suggestion is choosen via the items list
        /// </summary>
        event EventHandler<AutoCompleteEntryQuerySubmittedEventArgs> QuerySubmitted;
    }
}
