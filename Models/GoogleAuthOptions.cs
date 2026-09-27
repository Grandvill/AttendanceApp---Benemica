namespace AttendanceApp.Models;

// class untuk menyimpan status konfigurasi autentikasi Google
// Nilainya dihitung satu kali saat aplikasi dijalankan di Program.cs
//digunakan agar AccountController apakah Google Login dapat digunakan tanpa harus membaca ulang konfigurasi
public class GoogleAuthOptions
{
    public bool IsConfigured { get; init; }
}
