using System;

public interface IObserver<T> {
    void OnCompleted();
    void OnError(Exception exception);
    void OnNext(T value);
}
 
public interface IObservable<T> {
    IDisposable Subscribe(IObserver<T> observer);
}