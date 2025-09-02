namespace ToDoApi.Models
{
    public enum State
    {
        // Görev henüz aktif değil veya süresi geçtiği için kilitli.
        Kapali,  // Değeri: 0

        // Görev şu an aktif ve kullanıcı tarafından işaretlenebilir durumda.
        Acik,    // Değeri: 1

        // Görev kullanıcı tarafından başarıyla tamamlandı olarak işaretlendi.
        Isaretli // Değeri: 2
    }
}