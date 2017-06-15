namespace Serval.Transceive {
    public interface ITransmitter<TRecipient, TInput> {
        void SendAsync(TRecipient recipient, TInput obj);
    }
}
