namespace Serval.Communication.Pooling {
    public interface IPool<T> {
        T Retrieve();
        
        void Return(T item);
    }
}