using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiveTex.SDK.Client;
using LiveTex.SDK.Sample;

namespace LiveTex.SampleApp.ViewModel
{
	public class AbuseViewModel
		: ViewModel
	{
		private string _contact;
		public string Contact
		{
			get { return _contact; }
			set
			{
				if (SetValue(ref _contact, value))
				{ 
					AbuseCommand.RiseCanExecuteChanged();
				}
			}
		}

		private string _message;
		public string Message
		{
			get { return _message; }
			set
			{
				if (SetValue(ref _message, value))
				{
					AbuseCommand.RiseCanExecuteChanged();
				}
			}
		}

		private DelegateCommand _abuseCommand;
		public DelegateCommand AbuseCommand
		{
			get
			{
				if(_abuseCommand == null)
				{
					_abuseCommand = new DelegateCommand(Abuse, IsAbuseAllowed);
				}

				return _abuseCommand;
			}
		}

		private bool IsAbuseAllowed()
		{
			return !string.IsNullOrWhiteSpace(Contact)
				&& !string.IsNullOrWhiteSpace(Message);
		}

		private async void Abuse()
		{
			var result = await WrapRequest(() => Client.AbuseAsync(new Abuse { Contact = Contact, Message = Message }));
			if(result)
			{
				App.RootFrame.GoBack();
			}
		}
	}
}
