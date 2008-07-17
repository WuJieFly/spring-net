#region License

/*
 * Copyright 2002-2008 the original author or authors.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

#endregion

using System;
using System.Messaging;
using Spring.Context;
using Spring.Messaging.Core;
using Spring.Messaging.Support.Converters;
using Spring.Objects.Factory.Support;
using Spring.Transaction.Support;

namespace Spring.Messaging.Support
{
    public class QueueUtils
    {

        /// <summary>
        /// Registers the default message converter with the application context.
        /// </summary>
        /// <param name="applicationContext">The application context.</param>
        /// <returns>The name of the message converter to use for lookups with 
        /// <see cref="DefaultMessageQueueFactory"/>.
        /// </returns>
        public static string RegisterDefaultMessageConverter(IApplicationContext applicationContext)
        {                
            //Create a default message converter to use.
            RootObjectDefinition rod = new RootObjectDefinition(typeof(XmlMessageConverter));
            rod.PropertyValues.Add("TargetTypes", new Type[] { typeof(String) });
            rod.IsSingleton = false;
            IConfigurableApplicationContext ctx = (IConfigurableApplicationContext)applicationContext;
            DefaultListableObjectFactory of = (DefaultListableObjectFactory)ctx.ObjectFactory;
            string messageConverterObjectName = "__XmlMessageConverter__";
            if (!applicationContext.ContainsObjectDefinition(messageConverterObjectName))
            {
                of.RegisterObjectDefinition(messageConverterObjectName, rod);
            }
            return messageConverterObjectName;
            
        }
        public static MessageQueueTransaction GetMessageQueueTransaction(IResourceFactory resourceFactory)
        {
            MessageQueueResourceHolder resourceHolder =
                (MessageQueueResourceHolder)
                TransactionSynchronizationManager.GetResource(
                    MessageQueueTransactionManager.CURRENT_TRANSACTION_SLOTNAME);
            if (resourceHolder != null)
            {
                return resourceHolder.MessageQueueTransaction;
            }
            if (!TransactionSynchronizationManager.SynchronizationActive)
            {
                return null;
            }
            throw new NotImplementedException();
            /*
                MessageQueueResourceHolder resourceHolderToUse = resourceHolder;
                if (resourceHolderToUse == null)
                {
                    resourceHolderToUse = new MessageQueueResourceHolder(new MessageQueueTransaction());
                }
                if (resourceHolderToUse != resourceHolder)
                {
                    TransactionSynchronizationManager.RegisterSynchronization(
                            new MessageQueueResourceSynchronization(resourceHolderToUse, resourceFactory.SynchedLocalTransactionAllowed));
                    resourceHolderToUse.SynchronizedWithTransaction = true;
                    TransactionSynchronizationManager.BindResource(MessageQueueTransactionManager.CURRENT_TRANSACTION_SLOTNAME, resourceHolderToUse);
                }
                return resourceHolderToUse.MessageQueueTransaction;*/
        }
    }

    internal class MessageQueueResourceSynchronization : ITransactionSynchronization
    {
        private object resourceKey;

        private MessageQueueResourceHolder resourceHolder;

        private bool holderActive = true;

        public MessageQueueResourceSynchronization(MessageQueueResourceHolder resourceHolder, object resourceKey)
        {
            this.resourceHolder = resourceHolder;
            this.resourceKey = resourceKey;
        }

        #region ITransactionSynchronization Members

        public void Suspend()
        {
            if (holderActive)
            {
                TransactionSynchronizationManager.UnbindResource(resourceKey);
            }
        }

        public void Resume()
        {
            if (holderActive)
            {
                TransactionSynchronizationManager.BindResource(resourceKey, resourceHolder);
            }
        }

        public void BeforeCommit(bool readOnly)
        {
            throw new NotImplementedException();
        }

        public void AfterCommit()
        {
            throw new NotImplementedException();
        }

        public void BeforeCompletion()
        {
            TransactionSynchronizationManager.UnbindResource(resourceKey);
            holderActive = false;
            //this.resourceHolder.closeAll();
            throw new NotImplementedException();
        }

        public void AfterCompletion(TransactionSynchronizationStatus status)
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    public interface IResourceFactory
    {
        bool SynchedLocalTransactionAllowed { get; }
    }
}