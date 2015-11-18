using System;
using System.Threading.Tasks;
using LiveTex.SampleApp.LiveTex;
using LiveTex.SDK.Client;

namespace LiveTex.SampleApp.Wrappers
{
	public abstract class ListItemWrapper
	{
		public abstract string Description { get; }
	}

	public class ListItemWrapper<T>
		: ListItemWrapper
	{
		public ListItemWrapper(T sourceObject, string description)
		{
			SourceObject = sourceObject;
			Description = description ?? "";
		}

		public override string Description { get; }

		public T SourceObject { get; }
	}

	public sealed class OfflineConversationWrapper
	{
		private OfflineConversationWrapper(OfflineConversation conversation, string employeeName, string employeeAvatar)
		{
			Guard.NotNull(conversation, nameof(conversation));
			
			Conversation = conversation;
			EmployeeName = employeeName;
			EmployeeAvatar = employeeAvatar;
		}

		public static async Task<OfflineConversationWrapper> CreateAsync(OfflineConversation conversation)
		{
			if (conversation.Status == OfflineConversationStatus.Close)
			{
				return new OfflineConversationWrapper(conversation, "Диалог закрыт", null);
			}

			Employee employee = null;
			if(conversation.Route != null)
			{
				try
				{
					employee = await LiveTexClient.Client.GetEmployeeAsync(conversation.Route.MemberID.ToString());
				}
				catch(Exception)
				{
					//ignore
				}
			}

			if (employee == null)
			{
				return new OfflineConversationWrapper(conversation, "Оператор не назначен", null);
			}

			return new OfflineConversationWrapper(conversation, employee.Firstname + " " + employee.Lastname, employee.Avatar);
		}

		public OfflineConversation Conversation { get; }
		public string EmployeeAvatar { get; }
		public string EmployeeName { get; }
	}
}
