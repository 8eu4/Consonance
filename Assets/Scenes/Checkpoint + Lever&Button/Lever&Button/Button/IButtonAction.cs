public interface IButtonAction
{
    void OnButtonPressed();     // Button ditekan → aktif
    void OnButtonReleased();    // Setelah delay → mati
}