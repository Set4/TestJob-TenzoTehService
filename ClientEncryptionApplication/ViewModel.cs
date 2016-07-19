using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ClientEncryptionApplication
{
   
        public interface IViewModel : INotifyPropertyChanged
        {
            OperationRequest? SelectedOperation { get; set; }
            void SendMessage();
        }



    public class ViewModel : Notifier, IViewModel
    {
        private string _message;
        public string Message
        {
            get {
                if (_message == null)
                {
                    return String.Empty;
                }
                else
                {
                   return _message;
                }
            }
            set
            {
                    _message = value;
            }
        }


 public ObservableCollection<DeEncryptionResult> DeEncryptionCollection { get; set; }

 public List<OperationRequest> Operations { get { return _model.Operations; } }



 private readonly IModel _model;


      



        public ViewModel(IModel model)
        {
            _model = model;
            _model.DeEncryptionResultUpdated += _model_DeEncryptionResultUpdated;

            DeEncryptionCollection = new ObservableCollection<DeEncryptionResult>();



            _SendCommand = new SendCommand(this);


        }


        public bool _sendEnabled;
        public bool SendEnabled
        {
            get { return _sendEnabled; }
            set
            {
               _sendEnabled = value;
            }
        }

        OperationRequest _selectedOperation;

        public OperationRequest? SelectedOperation
        {
            get { return _selectedOperation; }
            set
            {
                if (value == null)
                {
                    SendEnabled = false;
                }
                else
                {
                    _selectedOperation = (OperationRequest)value;
                    SendEnabled = true;
                    NotifyPropertyChanged("SendEnabled");
                }
            }
        }








        public const string SELECTED_PROJECT_PROPERRTY_NAME
            = "SelectedOperation";

      private readonly ICommand _SendCommand;

        public ICommand SendCommand
        {
            get { return _SendCommand; }
        }


        private void _model_DeEncryptionResultUpdated(object sender, DeEncryptionResultEventArgs e)
        {
            DeEncryptionCollection.Add(e._DeEncryptionResult);
            //coll update!!
        }

        public void SendMessage()
        {
          
            _model.UpdateDeEncryptionResult(_selectedOperation,Message);
        }
    }


    internal class SendCommand : ICommand
    {
        private const int ARE_EQUAL = 0;
        private const int NONE_SELECTED = -1;
        private IViewModel _vm;

        public SendCommand(IViewModel viewModel)
        {
            _vm = viewModel;
            _vm.PropertyChanged += vm_PropertyChanged;
            
        }

        private void vm_PropertyChanged(object sender,
            PropertyChangedEventArgs e)
        {
            if (string.Compare(e.PropertyName,
                               ViewModel.
                               SELECTED_PROJECT_PROPERRTY_NAME)
                == ARE_EQUAL)
            {
                CanExecuteChanged(this, new EventArgs());
            }
        }



        public bool CanExecute(object parameter)
        {
            if (_vm.SelectedOperation == null)
                return false;
            else
                return true;
        }

        public event EventHandler CanExecuteChanged
            = delegate { };

        public void Execute(object parameter)
        {
            _vm.SendMessage();
        }
    }
}
