using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using Prism.Commands;
using Prism.Mvvm;
using WebSocketSharp;

namespace PrismWSEchoSample.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private string _title = "WS Echo Sample";
        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }

        public MainWindowViewModel()
        {
            InitializeViewModel();
        }

        private void InitializeViewModel()
        {
            ConnectCommand = new DelegateCommand(Connect);
            SendCommand = new DelegateCommand(Send);
            DisconnectCommand = new DelegateCommand(Disconnect);
        }

        private void Send()
        {
            SendMessage = "send:" + SendWord; 
            _mySocket?.Send(SendWord);
        }

        private void Connect()
        {
            DisconnectCommand.Execute();

            _mySocket = new WebSocket(WSEndPoint);

            var observer = Observable
                .Using(
                    () => _mySocket,
                    ws =>
                        Observable
                            .FromEventPattern<EventHandler<MessageEventArgs>, MessageEventArgs>(
                                handler => ws.OnMessage += handler,
                                handler => ws.OnMessage -= handler));

            _myDisposable = observer.Subscribe(ep =>
            {
                Status = "WS Recieved";
                ReceivedMessage = "received:" + ep.EventArgs.Data;
            });

            _mySocket.Connect();

            Status = "WS Connected.";

        }

        private void Disconnect()
        {
            _myDisposable?.Dispose();
            _myDisposable = null;

            //ObservableのUsingでCloseされるので、能動的にCloseしなくても良い
            //_mySocket?.Close();

            _mySocket = null;

            Status = "WS Closed.";
        }

        private WebSocket _mySocket = null;

        private IDisposable _myDisposable = null;

        private string _myWSEndPoint = "ws://echo.websocket.org";
        public string WSEndPoint
        {
            get { return _myWSEndPoint; }
            set { SetProperty(ref _myWSEndPoint, value); }
        }

        private string _myStatus = "not connected.";
        public string Status
        {
            get { return _myStatus; }
            set { SetProperty(ref _myStatus, value); }
        }

        private string _mySendWord = "send word.";
        public string SendWord
        {
            get { return _mySendWord; }
            set { SetProperty(ref _mySendWord, value); }
        }

        private string _mySendMessage = "not send.";
        public string SendMessage
        {
            get { return _mySendMessage; }
            set { SetProperty(ref _mySendMessage, value); }
        }

        private string _myReceivedMessage = "not received.";
        public string ReceivedMessage
        {
            get { return _myReceivedMessage; }
            set { SetProperty(ref _myReceivedMessage, value); }
        }

        public DelegateCommand ConnectCommand { get; set; }

        public DelegateCommand SendCommand { get; set; }

        public DelegateCommand DisconnectCommand { get; set; }
    }
}
