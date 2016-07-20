using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientEncryptionApplication
{
    public class Notifier : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        protected void NotifyPropertyChanged(
            string propertyName)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }

  

    public class DeEncryptionResultEventArgs : EventArgs
    {
        public DeEncryptionResult _DeEncryptionResult { get; set; }

        public DeEncryptionResultEventArgs(DeEncryptionResult response)
        {
            _DeEncryptionResult = response;
        }
    }


    public interface IModel
    {
      /// <summary>
      /// доступные операции над сообщением
      /// </summary>
        List<OperationRequest> Operations { get; set; }

        /// <summary>
        /// событие получения ответа от сервера
        /// </summary>
        event EventHandler<DeEncryptionResultEventArgs> DeEncryptionResultUpdated;

        /// <summary>
        /// запрос к серверу
        /// </summary>
        /// <param name="operation">операция</param>
        /// <param name="message">сообщение</param>
        void UpdateDeEncryptionResult(OperationRequest operation, string message);
    }


    public class Model : IModel
    {
     
        
        public List<OperationRequest> Operations { get; set; }


        public event EventHandler<DeEncryptionResultEventArgs>  DeEncryptionResultUpdated  = delegate { };

        public Model()
        {
             //~~~
            Operations = new List<OperationRequest>() { OperationRequest.Encoding, OperationRequest.Decoding };
        }

        public async void UpdateDeEncryptionResult(OperationRequest operation, string message)
        {
           
            DeEncryptionResult res= await GetDeEncryptionResult(operation, message);
           
            DeEncryptionResultUpdated(this,
                new DeEncryptionResultEventArgs(res));
        }

        private async Task<DeEncryptionResult> GetDeEncryptionResult(OperationRequest operation, string message )
        {
            DeEncryptionResult res = await new ServerHandler().Handler(new Request(operation, message), new XMLSerializationProvider());
            return res;
        }
    }



}
