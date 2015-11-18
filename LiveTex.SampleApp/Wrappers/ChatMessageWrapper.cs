using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using LiveTex.SDK.Client;

namespace LiveTex.SampleApp.Wrappers
{
	public enum ChatMessageType
	{
		Text,
		File,
		Hold,
		Typing
	}

	public class ChatMessageWrapper
		: INotifyPropertyChanged, IDisposable
	{
		private readonly CancellationTokenSource _cts = new CancellationTokenSource();

		public ChatMessageWrapper(string message)
		{
			TimeStamp = DateTime.Now;

			MessageType = ChatMessageType.Text;
			
			Message = message;
			IsIncomingMessage = false;

			Status = "отправляется";
		}

		public ChatMessageWrapper(OfflineMessage offlineMessage)
		{
			Guard.NotNull(offlineMessage, nameof(offlineMessage));

			MessageType = offlineMessage.MessageType == SDK.Client.MessageType.Text
				? ChatMessageType.Text
				: ChatMessageType.File;

			TimeStamp = offlineMessage.Timestamp;

			MessageID = offlineMessage.Id;

			if(MessageType == ChatMessageType.File)
			{
				SetUri(offlineMessage.Url);
			}
			else
			{
				Message = offlineMessage.Message;
			}

			IsIncomingMessage = offlineMessage.SenderID != null;

			Status = TimeStamp?.ToString("h:mm d MMM yyyy");
        }

		public ChatMessageWrapper(TextMessage textMessage)
		{
			if(textMessage == null)
			{
				throw new ArgumentNullException(nameof(textMessage));
			}

			MessageType = ChatMessageType.Text;
			TimeStamp = textMessage.Timestamp;
			
			MessageID = textMessage.Id;
			Message = textMessage.Text;
			IsIncomingMessage = textMessage.SenderID != null;

			Status = TimeStamp?.ToString("h:mm d MMM yyyy");
		}

		public ChatMessageWrapper(FileMessage fileMessage)
		{
			if (fileMessage == null)
			{
				throw new ArgumentNullException(nameof(fileMessage));
			}

			MessageType = ChatMessageType.File;

			MessageID = fileMessage.Id;
			Message = fileMessage.Text;
			TimeStamp = fileMessage.Timestamp;
			IsIncomingMessage = true;

			SetUri(fileMessage.Url);

			Status = TimeStamp?.ToString("h:mm d MMM yyyy");
		}

		public ChatMessageWrapper(HoldMessage holdMessage)
		{
			if (holdMessage == null)
			{
				throw new ArgumentNullException(nameof(holdMessage));
			}

			MessageType = ChatMessageType.Hold;

			Message = holdMessage.Text;
			TimeStamp = holdMessage.Timestamp;
			IsIncomingMessage = true;

			Status = TimeStamp?.ToString("h:mm d MMM yyyy");
		}

		public ChatMessageWrapper(TypingMessage typingMessage)
		{
			if (typingMessage == null)
			{
				throw new ArgumentNullException(nameof(typingMessage));
			}

			MessageType = ChatMessageType.Typing;
			IsIncomingMessage = true;

			StartTypingAnimation().LogAsyncError();
		}

		public DateTime? TimeStamp { get; private set; }

		private string _messageID;
		public string MessageID
		{
			get
			{
				return _messageID;
			}
			private set
			{
				if(string.Equals(_messageID, value, StringComparison.OrdinalIgnoreCase))
				{
					return;
				}

				_messageID = value;
				OnPropertyChanged();
			}
		}

		private string _message;
		public string Message
		{
			get { return _message; }
			private set
			{
				if(string.Equals(_message, value))
				{
					return;
				}

				_message = value;
				OnPropertyChanged();
			}
		}

		private string _status;
		public string Status
		{
			get { return _status; }
			private set
			{
				if (string.Equals(_status, value, StringComparison.Ordinal))
				{
					return;
				}

				_status = value;

				OnPropertyChanged();
			}
		}
		
		public bool IsIncomingMessage { get; private set; }
		public string Uri { get; private set; }
		public string FileName{get;private set;}
		
		public ChatMessageType MessageType { get; }

		#region INotifyPropertyChanged

		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		#endregion

		private void SetUri(string uri)
		{
			if (string.IsNullOrWhiteSpace(uri))
			{
				Uri = null;
				FileName = null;

				return;
			}

			if(uri.StartsWith("//"))
			{
				uri = "http:" + uri;
			}

			Uri = uri;

			try
			{
				FileName = Path.GetFileName(Uri);
			}
			catch
			{
				FileName = Uri;
			}
		}

		public void SetMassageID(string messageID)
		{
			MessageID = messageID;
		}

		public void MarkAsReceived()
		{
			Status = TimeStamp != null
				? "√ " + TimeStamp.Value.ToString("h:mm d MMM yyyy")
				: null;
		}

		public void MarkAsFailed()
		{
			Status = "не отправлено";
		}

		private async Task StartTypingAnimation()
		{
			while (true)
			{
				if (_cts.IsCancellationRequested)
				{
					return;
				}

				var text = Message + ".";
				if (text.Length > 3)
				{
					text = ".";
				}
				
				Message = text;

				await Task.Delay(500);
			}
		}

		public void Dispose()
		{
			_cts.Cancel();
		}
	}
}
