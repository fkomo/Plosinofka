using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Ujeby.Plosinofka.Engine.Network.Messages
{
	public enum MessageType : byte
	{
		Invalid = 0,
		JoinGame = 1,
		LeaveGame = 2,
	}

	public abstract class Message
	{
		static readonly Dictionary<MessageType, Type> MessageTypeMap = new Dictionary<MessageType, Type>
		{
			{ MessageType.JoinGame, typeof(JoinGame) },
			{ MessageType.LeaveGame, typeof(LeaveGame) },
		};

		// auto-generated in constructor
		[MessageField]
		public Guid Id { get; set; }

		[MessageField]
		public DateTime Created { get; set; }

		[MessageField]
		public Guid SessionId { get; set; }

		[MessageField]
		public Guid ClientId { get; set; }

		public int Size { get; set; }

		public ClientDescriptor From { get; internal set; }
		public abstract MessageType MessageType { get; }

		public Message()
		{
			Id = Guid.NewGuid();
			Created = DateTime.UtcNow;
		}

		public override string ToString()
		{
			return $"[{ Id.ToString("N") };{ Created.ToString("yyyyMMddHHmmssfff") };{ ClientId.ToString("N") }:{ SessionId.ToString("N") }]{ From }";
		}

		public static byte[] Serialize(Message message)
		{
			using (var memStream = new MemoryStream())
			{
				var bytes = BitConverter.GetBytes((UInt16)message.MessageType);
				memStream.Write(bytes, 0, bytes.Length);

				var messageFields = GetOrderedProperties(message.GetType());
				foreach (var messageField in messageFields)
				{
					if (messageField.PropertyType == typeof(Guid))
					{
						bytes = ((Guid)messageField.GetValue(message)).ToByteArray();
						memStream.Write(bytes, 0, bytes.Length);
					}
					else if (messageField.PropertyType == typeof(string))
					{
						var encodedString = Encoding.ASCII.GetBytes((string)messageField.GetValue(message));
						memStream.Write(BitConverter.GetBytes(encodedString.Length), 0, sizeof(int));
						memStream.Write(encodedString, 0, encodedString.Length);
					}
					else if (messageField.PropertyType == typeof(DateTime))
					{
						bytes = BitConverter.GetBytes(((DateTime)messageField.GetValue(message)).Ticks);
						memStream.Write(bytes, 0, bytes.Length);
					}
					else
						throw new NotSupportedException(messageField.PropertyType.Name);
				}

				return memStream.ToArray();
			};
		}

		public static Message Deserialize(byte[] messageBytes)
		{
			var size = messageBytes.Length;

			var messageTypeEnum = (MessageType)BitConverter.ToUInt16(messageBytes, 0);
			messageBytes = messageBytes.Skip(sizeof(UInt16)).ToArray();

			var messageType = MessageTypeMap[messageTypeEnum];
			var instance = Activator.CreateInstance(messageType) as Message;

			var messageFields = GetOrderedProperties(messageType);
			foreach (var messageField in messageFields)
			{
				if (messageField.PropertyType == typeof(Guid))
				{
					var length = Guid.Empty.ToByteArray().Length;
					messageField.SetValue(instance, new Guid(messageBytes.Take(length).ToArray()));
					messageBytes = messageBytes.Skip(length).ToArray();
				}
				else if (messageField.PropertyType == typeof(string))
				{
					var length = BitConverter.ToInt32(messageBytes, 0);
					messageField.SetValue(instance, Encoding.ASCII.GetString(messageBytes, sizeof(int), length));
					messageBytes = messageBytes.Skip(sizeof(int) + length).ToArray();
				}
				else if (messageField.PropertyType == typeof(DateTime))
				{
					messageField.SetValue(instance, new DateTime(BitConverter.ToInt64(messageBytes, 0)));
					messageBytes = messageBytes.Skip(sizeof(Int64)).ToArray();
				}
				else
					throw new NotSupportedException(messageField.PropertyType.Name);
			}

			instance.Size = size;
			return instance;
		}

		private static IEnumerable<PropertyInfo> GetOrderedProperties(Type messageType)
		{
			return messageType.GetProperties()
				.Where(p => p.CustomAttributes.Any(ca => ca.AttributeType == typeof(MessageFieldAttribute)))
				//.Where(p => !p.CustomAttributes.Single(ca => ca.AttributeType == typeof(MessageFieldAttribute)).NamedArguments.Any(na => na.MemberName == nameof(MessageFieldAttribute.Ignore) && (bool?)na.TypedValue .Value == true).TypedValue.Value as int?);
				//.OrderBy(p => p.CustomAttributes.First().NamedArguments.Single(a => a.MemberName == nameof(MessageFieldAttribute.Order)).TypedValue.Value as int?);
				.OrderBy(p => p.Name);
		}
	}
}
