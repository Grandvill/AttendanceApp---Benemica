namespace AttendanceApp.Models;

/// <summary>
/// Status konfigurasi Google OAuth, dihitung sekali saat startup di Program.cs.
/// Dipakai supaya tombol "Sign in with Google" bisa menampilkan pesan yang jelas
/// saat ClientId/ClientSecret belum diisi, tanpa membuat aplikasi gagal start.
/// </summary>
public class GoogleAuthOptions
{
    public bool IsConfigured { get; init; }
}
