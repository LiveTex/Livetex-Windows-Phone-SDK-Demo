using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace LiveTex.SampleApp.Wrappers
{
	public class ChatMessageCollection
		: ObservableCollection<ChatMessageWrapper>
	{
		private readonly HashSet<string> _messageIDs = new HashSet<string>();
		private readonly HashSet<string> _confirmedIDs = new HashSet<string>();

		protected override void InsertItem(int index, ChatMessageWrapper item)
		{
			if (string.IsNullOrWhiteSpace(item.MessageID))
			{
				if (IndexOf(item) >= 0)
				{
					return;
				}
			}
			else if(!_messageIDs.Add(item.MessageID))
			{
				return;
			}

			base.InsertItem(index, item);

			if(_confirmedIDs.Contains(item.MessageID))
			{
				MarkAsReceived(item.MessageID);
			}
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

		public void MarkAsReceived(string messageID)
		{
			if(string.IsNullOrWhiteSpace(messageID))
			{
				return;
			}

			if(_messageIDs.Contains(messageID))
			{
				var item = this.FirstOrDefault(w => string.Equals(w.MessageID, messageID, StringComparison.Ordinal));
				if(item != null)
				{
					item.MarkAsReceived();
					return;
				}
			}

			_confirmedIDs.Add(messageID);
		}

		public void UpdateMessage(ChatMessageWrapper messageWrapper)
		{
			if(IndexOf(messageWrapper) < 0)
			{
				return;
			}

			if(string.IsNullOrWhiteSpace(messageWrapper.MessageID))
			{
				return;
			}

			_messageIDs.Add(messageWrapper.MessageID);
		}
	}
}
