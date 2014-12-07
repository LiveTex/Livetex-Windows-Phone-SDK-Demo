using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
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
		private readonly DateTime? _timeStamp;
		private readonly ChatMessageType _messageType;
		private readonly CancellationTokenSource _cts = new CancellationTokenSource();

		public ChatMessageWrapper(string message)
		{
			_timeStamp = DateTime.Now;

			_messageType = ChatMessageType.Text;
			
			Message = message;
			IsIncomingMessage = false;

			Status = "отправляется";
		}

		public ChatMessageWrapper(TextMessage textMessage)
		{
			if(textMessage == null)
			{
				throw new ArgumentNullException("textMessage");
			}

			_messageType = ChatMessageType.Text;
			_timeStamp = textMessage.Timestamp;
			
			MessageID = textMessage.Id;
			Message = textMessage.Text;
			IsIncomingMessage = textMessage.SenderID != null;

			Status = _timeStamp != null
				? _timeStamp.Value.ToString("h:mm d MMM yyyy")
				: null;
		}

		public ChatMessageWrapper(FileMessage fileMessage)
		{
			if (fileMessage == null)
			{
				throw new ArgumentNullException("fileMessage");
			}

			_messageType = ChatMessageType.File;

			MessageID = fileMessage.Id;
			Message = fileMessage.Text;
			_timeStamp = fileMessage.Timestamp;
			IsIncomingMessage = true;
			Uri = fileMessage.Url;

			Status = _timeStamp != null
				? _timeStamp.Value.ToString("h:mm d MMM yyyy")
				: null;
		}

		public ChatMessageWrapper(HoldMessage holdMessage)
		{
			if (holdMessage == null)
			{
				throw new ArgumentNullException("holdMessage");
			}

			_messageType = ChatMessageType.Hold;

			Message = holdMessage.Text;
			_timeStamp = holdMessage.Timestamp;
			IsIncomingMessage = true;

			Status = _timeStamp != null
				? _timeStamp.Value.ToString("h:mm d MMM yyyy")
				: null;
		}

		public ChatMessageWrapper(TypingMessage typingMessage)
		{
			if (typingMessage == null)
			{
				throw new ArgumentNullException("typingMessage");
			}

			_messageType = ChatMessageType.Typing;
			IsIncomingMessage = true;

			StartTypingAnimation();
		}

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
		public string Status { get; private set; }
		public bool IsIncomingMessage { get; private set; }
		public string Uri { get; private set; }

		public ChatMessageType MessageType
		{
			get { return _messageType; }
		}

		#region INotifyPropertyChanged

		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			var handler = PropertyChanged;
			if (handler != null)
			{
				handler(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		#endregion

		public void SetMassageID(string messageID)
		{
			MessageID = messageID;
		}

		public void MarkAsReceived()
		{
			Status = _timeStamp != null
				? _timeStamp.Value.ToString("h:mm d MMM yyyy")
				: null;
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
