using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace LiveTex.SampleApp.Wrappers
{
	public class ChatMessageCollection
		: ObservableCollection<ChatMessageWrapper>
	{
		private readonly HashSet<string> _messageIDs = new HashSet<string>(); 

		protected override void InsertItem(int index, ChatMessageWrapper item)
		{
			if(!_messageIDs.Add(item.MessageID))
			{
				return;
			}

			base.InsertItem(index, item);
		}

		protected override void ClearItems()
		{
			_messageIDs.Clear();
			base.ClearItems();
		}

		protected override void RemoveItem(int index)
		{
			if(Count > index)
			{
				_messageIDs.Remove(this[index].MessageID);
			}

			base.RemoveItem(index);
		}
	}
}
