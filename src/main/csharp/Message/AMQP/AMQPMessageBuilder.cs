﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Apache.NMS;
using Amqp.Types;
using Amqp.Framing;

namespace NMS.AMQP.Message.AMQP
{
    using Util;
    using Cloak;
    
    class AMQPMessageBuilder
    {
        public static IMessage CreateProviderMessage(MessageConsumer consumer, Amqp.Message message)
        {
            IMessage msg = null;
            msg = CreateFromMessageAnnontations(consumer, message);
            if(msg == null)
            {
                msg = CreateFromMessageBody(consumer, message);
            }
            if(msg == null)
            {
                throw new NMSException("Could not create NMS Message.");
            }
            return msg;
        }

        private static IMessage CreateFromMessageBody(MessageConsumer consumer, Amqp.Message message)
        {
            IMessage msg = null;
            object body = message.Body;
            if(body == null)
            {
                if (IsContentType(SymbolUtil.SERIALIZED_JAVA_OBJECT_CONTENT_TYPE, message))
                {
                    msg = CreateObjectMessage(consumer, message);
                }
                else if (IsContentType(SymbolUtil.OCTET_STREAM_CONTENT_TYPE, message) || IsContentType(null, message))
                {
                    msg = CreateBytesMessage(consumer, message);
                }
                else
                {
                    Symbol contentType = GetContentType(message);
                    if(contentType != null)
                    {
                        msg = CreateTextMessage(consumer, message);
                    }
                    else
                    {
                        msg = CreateMessage(consumer, message);
                    }
                }
            }
            else if (message.BodySection is Data)
            {
                if(IsContentType(SymbolUtil.OCTET_STREAM_CONTENT_TYPE, message) || IsContentType(null, message))
                {
                    msg = CreateBytesMessage(consumer, message);
                }
                else if (IsContentType(SymbolUtil.SERIALIZED_JAVA_OBJECT_CONTENT_TYPE, message))
                {
                    msg = CreateObjectMessage(consumer, message);
                }
                else
                {
                    Symbol contentType = GetContentType(message);
                    if(contentType != null)
                    {
                        msg = CreateTextMessage(consumer, message);
                    }
                    else
                    {
                        msg = CreateBytesMessage(consumer, message);
                    }
                }
            }
            else if (message.BodySection is AmqpSequence)
            {
                msg = CreateObjectMessage(consumer, message);
            }
            else if (body is string)
            {
                msg = CreateTextMessage(consumer, message);
            }
            else if (body is byte[])
            {
                msg = CreateBytesMessage(consumer, message);
            }
            else
            {
                msg = CreateObjectMessage(consumer, message);
            }

            return msg;
        }

        private static Symbol GetContentType(Amqp.Message message)
        {
            Properties msgProps = message.Properties;
            if (msgProps == null)
            {
                return null;
            }
            else
            {
                return msgProps.ContentType;
            }
        }

        private static bool IsContentType(Symbol type, Amqp.Message message)
        {
            Symbol contentType = GetContentType(message);
            if (contentType == null)
            {
                return type == null;
            }
            else
            {
                return type.Equals(contentType);
            }
        }

        private static IMessage CreateFromMessageAnnontations(MessageConsumer consumer, Amqp.Message message)
        {
            IMessage msg = null;
            object objVal = message.MessageAnnotations[SymbolUtil.JMSX_OPT_MSG_TYPE];
            if(objVal != null && objVal is byte)
            {
                byte type = (byte)objVal;
                switch (type)
                {
                    case MessageSupport.JMS_TYPE_MSG:
                        msg = CreateMessage(consumer, message);
                        break;
                    case MessageSupport.JMS_TYPE_BYTE:
                        msg = CreateBytesMessage(consumer, message);
                        break;
                    case MessageSupport.JMS_TYPE_TXT:
                        msg = CreateTextMessage(consumer, message);
                        break;
                    case MessageSupport.JMS_TYPE_OBJ:
                        msg = CreateObjectMessage(consumer, message);
                        break;
                    case MessageSupport.JMS_TYPE_STRM:
                        msg = CreateStreamMessage(consumer, message);
                        break;
                    case MessageSupport.JMS_TYPE_MAP:
                        msg = CreateMapMessage(consumer, message);
                        break;
                    default:
                        throw new NMSException("Unsuported Msg Annontation type: " + type);
                }
            }
            return msg;
        }

        private static IMessage CreateMessage(MessageConsumer consumer, Amqp.Message message)
        {
            IMessageCloak cloak = new AMQPMessageCloak(consumer, message);
            return new Message(cloak);
        }

        private static IMessage CreateTextMessage(MessageConsumer consumer, Amqp.Message message)
        {
            ITextMessageCloak cloak = new AMQPTextMessageCloak(consumer, message);
            return new TextMessage(cloak);
        }

        private static IMessage CreateStreamMessage(MessageConsumer consumer, Amqp.Message message)
        {
            // TODO implement Stream Message
            return null;
        }

        private static IMessage CreateObjectMessage(MessageConsumer consumer, Amqp.Message message)
        {
            // TODO implement Object Message
            return null;
        }

        private static IMessage CreateMapMessage(MessageConsumer consumer, Amqp.Message message)
        {
            // TODO implement Object Message
            return null;
        }

        private static IMessage CreateBytesMessage(MessageConsumer consumer, Amqp.Message message)
        {
            IBytesMessageCloak cloak = new AMQPBytesMessageCloak(consumer, message);
            return new BytesMessage(cloak);
        }
        
    }
}
