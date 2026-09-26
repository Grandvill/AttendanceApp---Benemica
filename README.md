# AttendanceApp

Aplikasi web sederhana untuk **mengelola absensi (attendance) karyawan**.

Cara kerjanya singkat saja:

1. Anda punya file Excel berisi daftar absensi karyawan.
2. File itu di-upload ke aplikasi.
3. Aplikasi menampilkan isinya dalam bentuk tabel yang **bisa diedit** (jam masuk, jam keluar, status cuti).
4. Klik **Save**, datanya tersimpan permanen di database SQL Server.
5. Kapan saja Anda bisa membuka kembali data absensi **per tanggal**.

Untuk masuk ke aplikasi, pengguna harus **login dengan akun Google**.

> Cocok dipakai kalau proses absensi masih dicatat di Excel, tetapi ingin ada penyimpanan data yang rapi dan bisa dibuka ulang.

---

## Daftar isi

- [Fitur](#fitur)
- [Tech stack (teknologi yang dipakai)](#tech-stack-teknologi-yang-dipakai)
- [Struktur folder](#struktur-folder)
- [Prasyarat (alat yang harus diinstall dulu)](#prasyarat-alat-yang-harus-diinstall-dulu)
- [1. Cara install .NET](#1-cara-install-net)
- [2. Kalau membuat project dari nol](#2-kalau-membuat-project-dari-nol)
- [3. Kalau menjalankan dari hasil clone GitHub](#3-kalau-menjalankan-dari-hasil-clone-github)
- [4. Setup database (migration)](#4-setup-database-migration)
- [5. Setup login Google (OAuth)](#5-setup-login-google-oauth)
- [6. Cara menjalankan aplikasi](#6-cara-menjalankan-aplikasi)
- [7. Cara memakai aplikasi](#7-cara-memakai-aplikasi)
- [8. Format file Excel](#8-format-file-excel)
- [9. Alur data aplikasi (flow)](#9-alur-data-aplikasi-flow)
- [10. Kalau ada error (troubleshooting)](#10-kalau-ada-error-troubleshooting)
- [11. Catatan penting](#11-catatan-penting)

---

## Fitur

| Fitur | Keterangan |
|---|---|
| Login Google | Masuk aplikasi memakai akun Google (OAuth 2.0). Tidak ada form username/password. |
| Logout | Keluar dari aplikasi dan menghapus sesi login (alamat `/Account/Logout`). |
| Upload Excel | Upload file absensi berformat `.xlsx`, lalu isinya dibaca otomatis. |
| Edit data sebelum simpan | Jam masuk (Attendance IN), jam keluar (Attendance OUT), dan pilihan Leave bisa diubah langsung di tabel. Employee ID dan Nama hanya bisa dilihat (readonly), karena dianggap data induk karyawan. |
| Hapus baris | Baris yang tidak ingin disimpan bisa dihapus di tabel sebelum klik Save. |
| Simpan ke database | Data disimpan ke SQL Server. Kalau kombinasi **ID + tanggal** sudah ada, datanya **diperbarui**, bukan dibuat dobel. |
| Lihat data tersimpan | Pilih tanggal lalu klik **Load** untuk memastikan data benar-benar sudah masuk database. |
| Laporan baris bermasalah | Baris Excel yang datanya tidak valid tidak menggagalkan seluruh file — baris itu dilewati dan dilaporkan. |
| Halaman Access Denied | Halaman yang muncul saat pengguna tidak punya izin mengakses halaman tertentu. |

---

## Tech stack (teknologi yang dipakai)

| Teknologi | Versi | Dipakai untuk |
|---|---|---|
| **.NET / C#** | .NET 10 (`net10.0`) | Bahasa dan platform utama aplikasi. |
| **ASP.NET Core MVC** | 10 | Kerangka kerja web: mengatur halaman (View), alur (Controller), dan data (Model). |
| **Razor View** | — | File `.cshtml`, yaitu tampilan HTML yang bisa disisipi kode C#. |
| **Entity Framework Core** (SqlServer) | `10.*` | Menghubungkan aplikasi C# ke database tanpa menulis query SQL manual. |
| **Entity Framework Core Design** | `10.*` | Alat bantu untuk membuat *migration* (pembuat/pengubah struktur tabel). |
| **SQL Server** | — | Database tempat data absensi disimpan (tabel `Attendances`). |
| **ClosedXML** | `0.105.1` | Membaca file Excel (`.xlsx`) dari dalam kode C#. |
| **Microsoft.AspNetCore.Authentication.Google** | `10.0.12` | Login memakai akun Google (Google OAuth). |
| **Cookie Authentication** | bawaan ASP.NET Core | Menyimpan status login pengguna setelah berhasil login Google. |
| **Bootstrap** | 5 (folder `wwwroot/lib`) | Membuat tampilan rapi dan responsif tanpa menulis banyak CSS. |
| **jQuery + jquery-validation** | (folder `wwwroot/lib`) | Dipakai oleh validasi form bawaan ASP.NET Core dan skrip halaman. |
| **JavaScript (vanilla)** | — | Menambah/menghapus baris tabel dan menomori ulang nama field sebelum Save. |
| **dotnet-ef CLI** | 10.x | Perintah terminal untuk membuat migration dan mengirimkannya ke database. |

Semua package di atas sudah terdaftar di dalam `AttendanceApp.csproj`. Jadi kalau Anda **clone** project ini, Anda **tidak perlu** menginstall package satu per satu (cukup `dotnet restore`).

---

## Struktur folder

```
AttendanceApp---Benemica/
├─ Controllers/            Alur logika tiap halaman
│  ├─ AccountController.cs     Login Google, logout, access denied
│  ├─ AttendanceController.cs  Upload Excel, simpan data, tampilkan data per tanggal
│  └─ HomeController.cs        Halaman Home + halaman Error
├─ Data/
│  └─ ApplicationDBContext.cs  Penghubung ke database (DbContext)
├─ DTOs/
│  └─ AttendanceRowDto.cs      Bentuk data satu baris absensi (dipakai form, Excel, dan view)
├─ Models/
│  ├─ AttendanceModel.cs       Model tabel Attendance (nama class: Attendance)
│  ├─ GoogleAuthOptions.cs     Status apakah konfigurasi Google sudah diisi
│  └─ ErrorViewModel.cs        Data untuk halaman Error
├─ Services/
│  ├─ AttendanceService.cs          Simpan & ambil data absensi dari database
│  ├─ IExcelAttendanceReader.cs     Interface + class hasil baca Excel
│  └─ ExcelAttendanceReader.cs      Membaca file Excel memakai ClosedXML
├─ Migrations/                Riwayat perubahan struktur database
├─ Views/                     Tampilan halaman (.cshtml)
│  ├─ Account/                Login.cshtml, AccessDenied.cshtml
│  ├─ Attendance/             Index.cshtml (upload, tabel edit, save, load)
│  ├─ Home/                   Index.cshtml
│  └─ Shared/                 _Layout.cshtml (navbar & footer), Error.cshtml
├─ wwwroot/                   File statis: css, js, bootstrap, jquery
├─ Properties/
│  └─ launchSettings.json     Alamat & port saat aplikasi dijalankan
├─ appsettings.json           Connection string database + konfigurasi Google
├─ Program.cs                 Titik masuk aplikasi (daftar service, middleware, route)
└─ AttendanceApp.csproj       Daftar package dan versi .NET yang dipakai
```

## Prasyarat (alat yang harus diinstall dulu)

| Alat | Kenapa butuh | Catatan |
|---|---|---|
| **.NET 10 SDK** | Untuk menjalankan dan mem-build aplikasi | Wajib. Project ini memakai `net10.0`. |
| **SQL Server** | Tempat data absensi disimpan | SQL Server Express (gratis) sudah cukup, atau SQL Server versi lain yang Anda punya. |
| **Git** | Untuk mengambil (clone) kode project | Hanya perlu kalau mengambil dari GitHub. |
| **Visual Studio Code atau Visual Studio** | Editor/IDE untuk melihat dan mengedit kode | Opsional, tapi sangat membantu. |
| **SQL Server Management Studio (SSMS)** | Melihat isi tabel langsung di database | Opsional. |

Untuk melihat versi yang sudah terinstall, buka terminal (PowerShell) lalu jalankan:

```powershell
dotnet --version
dotnet --list-sdks
git --version
```

Kalau `dotnet --version` mengeluarkan angka `10.x` (contoh: `10.0.103`), berarti .NET 10 SDK sudah siap.

---

## 1. Cara install .NET

### Windows

1. Buka halaman resmi: <https://dotnet.microsoft.com/download/dotnet/10.0>
2. Pilih **SDK** (bukan Runtime) untuk Windows x64, lalu unduh dan jalankan installer-nya (klik *Next/Install* sampai selesai).
3. **Tutup dan buka ulang** terminal, lalu cek:

```powershell
dotnet --version
```

Kalau belum keluar angka `10.x`, artinya SDK belum terpasang dengan benar — coba restart komputer lalu cek lagi.

### Cara lain (lewat winget, kalau tersedia)

```powershell
winget install Microsoft.DotNet.SDK.10
```

### Sekalian percaya-kan sertifikat HTTPS (dipakai saat development)

Aplikasi ini bisa dijalankan dengan dua alamat:

- `http://localhost:5168` (dipakai `dotnet run` tanpa opsi)
- `https://localhost:7275` (dipakai `dotnet run --launch-profile https`)

Supaya browser tidak menampilkan peringatan "not secure" saat memakai alamat HTTPS:

```powershell
dotnet dev-certs https --trust
```

Akan muncul pop-up konfirmasi — klik **Yes**. Ini hanya perlu sekali di tiap komputer.

---

## 2. Kalau membuat project dari nol

> Bagian ini hanya untuk yang ingin membangun project dari awal.
> **Kalau Anda clone dari GitHub, lewati saja bagian ini dan langsung ke [bagian 3](#3-kalau-menjalankan-dari-hasil-clone-github).**

```bash
// 1. Buat project ASP.NET Core MVC
dotnet new mvc -n AttendanceApp

// 2. Install package database (SQL Server) + alat migration
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package Microsoft.EntityFrameworkCore.Tools

// 3. Install package pembaca Excel + login Google
dotnet add package ClosedXML
dotnet add package Microsoft.AspNetCore.Authentication.Google
```

Untuk membuat tabel database dari kode (migration):

```bash
// 4. Install tool EF (sekali saja per komputer)
dotnet tool install --global dotnet-ef

// 5. Kalau tool sudah pernah diinstall, cukup update
dotnet tool update --global dotnet-ef

// 6. Buat file migration (ini membuat folder "Migrations")
dotnet ef migrations add InitialCreate

// 7. Kirim ke database (ini yang benar-benar membuat database + tabel)
dotnet ef database update
```

> Di project ini langkah 6 **tidak perlu dijalankan lagi**, karena file migration `InitialCreate` sudah ada di folder `Migrations/`. Lihat penjelasan di [bagian 4](#4-setup-database-migration).

---

## 3. Kalau menjalankan dari hasil clone GitHub

Kalau Anda mengambil kode dari GitHub (bukan bikin dari nol), ikuti langkah berurutan di bawah ini.

### Langkah 1 — Clone repository

Buka terminal di folder tempat Anda ingin menyimpan project (contoh: `D:\Project`):

```bash
git clone https://github.com/Grandvill/AttendanceApp---Benemica.git
cd AttendanceApp---Benemica
```

### Langkah 2 — Download semua package

Ini tidak perlu `dotnet add package` satu per satu, karena semua package sudah tercatat di `AttendanceApp.csproj`. Cukup:

```bash
dotnet restore
```

### Langkah 3 — Install/update tool EF Core

Alat ini dipakai untuk membuat & mengirim struktur database:

```bash
// install pertama kali
dotnet tool install --global dotnet-ef

// kalau sudah pernah install, cukup update
dotnet tool update --global dotnet-ef
```

Cek apakah sudah terpasang:

```bash
dotnet ef --version
dotnet tool list --global
```

Kalau perintah `dotnet ef` muncul sebagai `dotnet-ef` versi `10.x`, berarti sudah siap.

> **Kalau muncul pesan `dotnet-ef is not recognized` / `'dotnet-ef' is not recognized`:** tutup lalu buka ulang terminal. Kalau masih sama, tambahkan folder tool .NET ke PATH Windows, biasanya `%USERPROFILE%\.dotnet\tools`.

### Langkah 4 — Periksa connection string database

Buka file `appsettings.json`, lihat bagian ini:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost\\SQLEXPRESS;Database=AttendanceDb;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

Artinya:

| Bagian | Arti |
|---|---|
| `Server=localhost\SQLEXPRESS` | Nama komputer + nama instance SQL Server yang dipakai. |
| `Database=AttendanceDb` | Nama database. Kalau belum ada, akan dibuat otomatis saat `database update`. |
| `Trusted_Connection=True` | Login ke SQL Server memakai akun Windows Anda (tidak perlu username/password). |
| `TrustServerCertificate=True` | Tidak menolak sertifikat lokal saat development. |

**Sesuaikan `Server` dengan komputer Anda.** Contoh yang sering dipakai:

| Kondisi | Tulisan di `Server=` |
|---|---|
| SQL Server Express (paling umum) | `localhost\SQLEXPRESS` |
| SQL Server default instance | `.` atau `localhost` atau `(local)` |
| SQL Server dengan nama instance khusus | `localhost\NAMAINSTANCE` |
| SQL Server LocalDB | `(localdb)\MSSQLLocalDB` |
| Database di komputer lain | `NAMAKOMPUTER\SQLEXPRESS` |

> Pastikan service SQL Server sedang **berjalan** (cek di `services.msc`, cari `SQL Server (SQLEXPRESS)` dan klik Start bila berhenti).

### Langkah 5 — Buat database dan tabel

```bash
dotnet ef database update
```

Perintah ini akan membuat database `AttendanceDb` beserta tabel `Attendances` sesuai file migration yang sudah ada.

> **Migration tidak perlu dibuat ulang.** Folder `Migrations/` sudah berisi `20260924132714_InitialCreate`, jadi `dotnet ef migrations add InitialCreate` **tidak dijalankan lagi** untuk project ini.
>
> Perintah `migrations add` hanya dipakai kalau nanti Anda **mengubah struktur tabel** (misalnya menambah kolom baru) — dan nama migration-nya juga berbeda, contoh: `dotnet ef migrations add TambahKolomShift`.

### Langkah 6 — Isi konfigurasi Google (wajib untuk bisa login)

Lihat [bagian 5](#5-setup-login-google-oauth) untuk langkah lengkapnya. Singkatnya:

```bash
dotnet user-secrets set "Authentication:Google:ClientId" "ISI_CLIENT_ID_ANDA"
dotnet user-secrets set "Authentication:Google:ClientSecret" "ISI_CLIENT_SECRET_ANDA"
```

### Langkah 7 — Jalankan aplikasi

```bash
dotnet run
```

Lalu buka alamat yang tertulis di terminal — secara default `http://localhost:5168`. Browser biasanya **terbuka otomatis** karena di `launchSettings.json` diatur `launchBrowser: true`.

### Ringkasan cepat (untuk yang sudah berpengalaman)

```bash
git clone https://github.com/Grandvill/AttendanceApp---Benemica.git
cd AttendanceApp---Benemica
dotnet restore
dotnet tool install --global dotnet-ef   // kalau belum pernah install
dotnet tool update --global dotnet-ef    // kalau sudah pernah install
dotnet ef database update
dotnet user-secrets set "Authentication:Google:ClientId" "..."
dotnet user-secrets set "Authentication:Google:ClientSecret" "..."
dotnet run
```

---

## 4. Setup database (migration)

### Apa itu migration? (penjelasan sederhana)

Bayangkan database itu lemari arsip. **Migration** adalah "catatan perubahan lemari" — kapan rak dibuat, kolom apa saja isinya. Dengan migration, struktur database bisa dibuat dan diubah **dari kode**, tidak perlu menggambar tabel manual di SQL Server.

- `dotnet ef migrations add <Nama>` → **membuat file catatan perubahan** (belum menyentuh database).
- `dotnet ef database update` → **menjalankan catatan itu ke database** (baru di sini tabel benar-benar dibuat/diubah).

### Kondisi project ini

| Hal | Status |
|---|---|
| Nama migration | `InitialCreate` (`Migrations/20260924132714_InitialCreate.cs`) |
| Tabel yang dibuat | `Attendances` |
| Perlu `migrations add` lagi? | **Tidak**, karena file migration sudah ada di repository |
| Yang perlu dijalankan | `dotnet ef database update` |

Tabel `Attendances` berisi kolom berikut:

| Kolom | Tipe | Keterangan |
|---|---|---|
| `Id` | bigint (auto) | Nomor unik tiap baris, dibuat otomatis. |
| `EmployeeId` | nvarchar(50) | ID karyawan (wajib). |
| `EmployeeName` | nvarchar(150) | Nama karyawan (wajib). |
| `AttendanceDate` | date | Tanggal absensi. |
| `AttendanceIn` | time (boleh kosong) | Jam masuk. |
| `AttendanceOut` | time (boleh kosong) | Jam keluar. |
| `IsLeave` | bit (default 0) | Penanda cuti/izin (Yes = 1, No = 0). |
| `CreatedAt` | datetime2 | Waktu data dibuat (otomatis dari `GETDATE()`). |
| `UpdatedAt` | datetime2 (boleh kosong) | Waktu data terakhir diperbarui. |

### Perintah yang sering dipakai

```bash
// Membuat file migration baru (hanya kalau struktur tabel berubah)
dotnet ef migrations add NamaPerubahan

// Mengirim migration ke database
dotnet ef database update

// Melihat daftar migration beserta status (sudah/belum masuk database)
dotnet ef migrations list

// Membatalkan migration terakhir yang belum masuk database
dotnet ef migrations remove

// Mengembalikan database ke migration tertentu
dotnet ef database update NamaMigration
```

### Kalau database tidak mau dibuat

1. Pastikan SQL Server sedang berjalan dan nama `Server=` di `appsettings.json` sudah benar.
2. Coba tes koneksi lewat SSMS dengan nama server yang sama.
3. Kalau akun Windows Anda tidak punya akses, ganti connection string menjadi mode SQL login:

```
Server=localhost\SQLEXPRESS;Database=AttendanceDb;User Id=sa;Password=PASSWORD_ANDA;TrustServerCertificate=True;
```

---

## 5. Setup login Google (OAuth)

Aplikasi ini tidak punya form username/password — satu-satunya cara login adalah lewat akun Google. Karena itu Anda perlu membuat "kunci aplikasi" di Google.

> Kalau konfigurasi ini belum diisi, aplikasi **tetap bisa dijalankan**, tetapi aplikasi akan menampilkan peringatan di log dan tombol *Sign in with Google* hanya menampilkan pesan:
> *"Login dengan Google belum dikonfigurasi..."*

### Langkah A — Buat kredensial di Google Cloud Console

1. Buka <https://console.cloud.google.com/> dan login dengan akun Google Anda.
2. Buat project baru (atau pilih project yang sudah ada) di bagian atas halaman.
3. Buka menu **APIs & Services → OAuth consent screen**, isi informasi dasar aplikasi (nama aplikasi, email pendukung), lalu simpan. Kalau diminta, tambahkan diri Anda sendiri sebagai *Test user*.
4. Buka menu **APIs & Services → Credentials**.
5. Klik **Create Credentials → OAuth client ID**.
6. Pilih **Application type: Web application**.
7. Isi **Name** bebas, contoh: `AttendanceApp Local`.
8. Di bagian **Authorized redirect URIs**, klik **Add URI**, lalu isi **persis** kedua alamat berikut (tambahkan satu per satu):

```
http://localhost:5168/signin-google
https://localhost:7275/signin-google
```

> `/signin-google` adalah alamat callback yang dipakai aplikasi (lihat `Program.cs` → `options.CallbackPath`).
> `dotnet run` tanpa opsi memakai profil `http` → alamatnya `http://localhost:5168`, jadi baris pertama itu yang paling sering dipakai. Baris `https` dipakai bila Anda menjalankan `dotnet run --launch-profile https`. Menambahkan keduanya membuat login tetap jalan di kedua cara.

9. Klik **Create**. Google akan menampilkan **Client ID** dan **Client Secret** — salin keduanya (Client Secret hanya bisa dilihat/di-copy saat itu atau kapan saja lewat menu Credentials).

### Langkah B — Simpan kredensial ke project (jangan di appsettings.json!)

Di folder project, jalankan:

```bash
dotnet user-secrets set "Authentication:Google:ClientId" "ISI_CLIENT_ID_ANDA"
dotnet user-secrets set "Authentication:Google:ClientSecret" "ISI_CLIENT_SECRET_ANDA"
```

Kenapa pakai `user-secrets`? Supaya Client Secret **tidak ikut ter-upload ke GitHub**. Nilainya disimpan di komputer Anda sendiri (di luar folder project), memakai `UserSecretsId` yang sudah ada di `AttendanceApp.csproj`.

Perintah bantu lainnya:

```bash
// melihat daftar secret yang tersimpan
dotnet user-secrets list

// menghapus satu secret
dotnet user-secrets remove "Authentication:Google:ClientSecret"
```

### Langkah C — Restart aplikasi

Setelah mengisi secret, hentikan aplikasi (Ctrl + C) lalu jalankan lagi:

```bash
dotnet run
```

Kalau sudah benar, halaman Login akan langsung mengarahkan Anda ke halaman pilih akun Google dan setelah berhasil Anda akan masuk ke halaman Home.

### Cara lain: environment variable (untuk server/production)

Kalau aplikasi dipasang di server, isi lewat environment variable (tanda `:` diganti `__`):

```powershell
$env:Authentication__Google__ClientId = "ISI_CLIENT_ID_ANDA"
$env:Authentication__Google__ClientSecret = "ISI_CLIENT_SECRET_ANDA"
dotnet run
```

---

## 6. Cara menjalankan aplikasi

Pastikan terminal sedang berada di folder project (folder yang berisi `AttendanceApp.csproj`).

### Cara biasa

```bash
dotnet run
```

Tunggu sampai muncul tulisan seperti:

```
Now listening on: http://127.0.0.1:5168
Application started. Press Ctrl+C to shut down.
```

Lalu buka `http://localhost:5168` di browser (biasanya browser terbuka otomatis).

### Cara development (otomatis restart saat kode diubah)

```bash
dotnet watch run
```

Setiap kali Anda menyimpan perubahan kode, aplikasi otomatis di-build ulang dan browser di-refresh.

### Alamat & port yang dipakai

| Profil | Alamat | Cara menjalankan |
|---|---|---|
| `http` (**default**) | `http://localhost:5168` | `dotnet run` |
| `https` | `https://localhost:7275` dan `http://localhost:5168` | `dotnet run --launch-profile https` |

> `dotnet run` tanpa opsi memakai profil **pertama** di `Properties/launchSettings.json`, yaitu profil `http`. Karena itu alamat default aplikasi ini adalah `http://localhost:5168`.
> Halaman pertama yang terbuka adalah **Login**, karena route default aplikasi diarahkan ke `Account/Login` (lihat `Program.cs`).

### Kalau ingin mengakses dari HP / komputer lain di jaringan yang sama

```bash
dotnet run --urls "http://0.0.0.0:5168"
```

Lalu buka `http://IP-KOMPUTER-ANDA:5168` dari perangkat lain. Jangan lupa, redirect URI Google juga perlu ditambah di Google Cloud Console (alamat IP yang sama + `/signin-google`).

### Menghentikan aplikasi

Tekan **Ctrl + C** di terminal.

### Build saja tanpa menjalankan

```bash
dotnet build
```

---

## 7. Cara memakai aplikasi

Berikut alur pemakaian dari sisi pengguna (tanpa perlu paham koding).

### Langkah 1 — Login

1. Buka alamat aplikasi yang tertulis di terminal — secara default `http://localhost:5168`.
2. Klik tombol **Sign in with Google**.
3. Pilih akun Google Anda → klik **Continue/Allow**.
4. Kalau berhasil, Anda akan diarahkan ke halaman **Home**.

Kalau tombol hanya menampilkan pesan *"Login dengan Google belum dikonfigurasi..."*, berarti langkah di [bagian 5](#5-setup-login-google-oauth) belum dijalankan.

### Langkah 2 — Buka menu Attendance File

Klik **Attendance File** di navbar atas.

Halaman ini terbagi menjadi beberapa bagian:

| Bagian | Fungsi |
|---|---|
| **1. Upload Excel** | Pilih file `.xlsx` lalu klik **Upload** untuk menampilkan isinya di tabel. |
| **2. Attendance Data** | Tabel yang berisi hasil upload dan bisa diedit. |
| **3. Saved data (load from SQL Server)** | Panel lipat (harus diklik dulu untuk membuka) yang memuat data tersimpan dari database berdasarkan tanggal. |

### Langkah 3 — Upload file Excel

1. Klik **Choose File / Pilih File**, pilih file absensi Anda (harus berformat `.xlsx`).
2. Klik **Upload**.
3. Kalau berhasil muncul notifikasi hijau, contoh: *"12 row(s) loaded from 'absensi-oktober.xlsx'..."*
4. Kalau ada baris yang datanya salah, muncul notifikasi kuning berisi daftar kesalahannya. Baris yang salah **tidak** ikut masuk tabel; baris yang benar tetap masuk.

### Langkah 4 — Edit data di tabel

Di tabel Anda bisa:

- Mengubah **Attendance IN** (jam masuk) — memakai pemilih waktu format `HH:mm`, contoh `08:30`.
- Mengubah **Attendance OUT** (jam keluar) — memakai pemilih waktu format `HH:mm`, contoh `17:00`.
- Mengubah **Leave** — pilih **Yes** (cuti/izin) atau **No** dari daftar pilihan.
- Menghapus baris yang tidak perlu disimpan, memakai tombol **Remove** di ujung kanan baris.
- Melihat jumlah baris di badge di sebelah kanan judul tabel.

> Kolom **ID** dan **Nama** berwarna abu-abu karena tidak bisa diedit (readonly). Kolom **Date** hanya ditampilkan sebagai teks, tidak bisa diubah dari tabel — tanggal mengikuti isi file Excel. Kalau perlu memperbaiki kolom ID/Nama/tanggal, perbaiki di file Excel lalu upload ulang.

### Langkah 5 — Klik Save

Klik tombol **Save**. Aturan penyimpanan:

- Data disimpan ke database SQL Server.
- Kalau **Employee ID + tanggal** yang sama sudah ada di database, data lama **diperbarui** (tidak jadi baris baru/dobel).
- Kalau belum ada, data **ditambahkan** sebagai baris baru.
- Setelah Save, aplikasi kembali menampilkan data pada tanggal tersebut, jadi Anda bisa langsung melihat hasilnya.

### Langkah 6 — Pastikan data sudah tersimpan

Di panel **3. Saved data (load from SQL Server)** (bagian bawah halaman — klik dulu judulnya untuk membuka):

1. Pilih tanggal pada kolom **Date**.
2. Klik **Load**.
3. Tabel akan menampilkan data pada tanggal itu **langsung dari database**.
4. Tombol **Today** untuk kembali ke tanggal hari ini (hari ini adalah tanggal default saat halaman pertama dibuka).

### Logout

Buka alamat `http://localhost:5168/Account/Logout` (sesuaikan alamat/port dengan yang tampil di terminal), atau tambahkan tautan sendiri di navbar (`Views/Shared/_Layout.cshtml`) bila ingin ada tombolnya. Setelah logout, Anda akan kembali ke halaman Login.

> Catatan: pada tampilan saat ini, navbar hanya berisi menu **Home** dan **Attendance File**, jadi logout diakses lewat alamat tersebut.

---

## 8. Format file Excel

File harus berformat **`.xlsx`** (Excel 2007 ke atas). File `.xls` (Excel lama) dan `.csv` **tidak** didukung.

### Aturan umum

- Hanya **sheet pertama** yang dibaca.
- **Baris pertama yang terisi** dianggap sebagai baris judul (header).
- Kolom dicari berdasarkan **nama judul**, bukan posisi. Jadi urutan kolom bebas.
- Judul kolom tidak peka huruf besar/kecil, spasi, dan tanda baca. Contoh: `Attendance IN`, `attendance in`, dan `AttendanceIn` dianggap sama.
- Baris kosong otomatis dilewati.
- Baris yang datanya salah **tidak membatalkan seluruh file** — baris itu dilewati dan pesannya ditampilkan di halaman, misalnya: `Row 7: Employee name is empty.`

### Kolom yang tersedia

| Kolom | Wajib? | Isi | Contoh |
|---|---|---|---|
| **ID** | **Wajib** | ID karyawan. Boleh berupa teks atau angka. | `EMP001` atau `10001` |
| **Nama** | **Wajib** | Nama karyawan. | `Budi Santoso` |
| **Date** | **Wajib** | Tanggal absensi. | `2026-09-24` |
| **Attendance IN** | Boleh kosong | Jam masuk. | `08:30` |
| **Attendance OUT** | Boleh kosong | Jam keluar. | `17:00` |
| **Leave** | Boleh kosong | Status cuti/izin. | `No` atau `Yes` |

### Judul kolom yang juga diterima (alias)

Aplikasi mengenali beberapa penulisan judul berikut, jadi Anda tidak wajib menulis persis sama:

| Kolom | Judul yang diterima |
|---|---|
| ID | `ID`, `EmployeeId`, `Employee`, `EmployeeCode`, `NIK` |
| Nama | `Nama`, `Name`, `EmployeeName`, `NamaKaryawan` |
| Date | `Date`, `Tanggal`, `AttendanceDate` |
| Attendance IN | `AttendanceIn`, `In`, `TimeIn`, `CheckIn`, `ClockIn`, `JamMasuk` |
| Attendance OUT | `AttendanceOut`, `Out`, `TimeOut`, `CheckOut`, `ClockOut`, `JamKeluar` |
| Leave | `Leave`, `IsLeave`, `Cuti`, `Izin` |

### Format tanggal & jam yang diterima

| Data | Format yang diterima |
|---|---|
| **Date** | Bisa berupa sel bertipe tanggal asli Excel, atau teks: `yyyy-MM-dd` (disarankan), `yyyy/MM/dd`, `dd/MM/yyyy`, `dd-MM-yyyy`. Contoh: `2026-09-24`. |
| **Attendance IN / OUT** | Sel bertipe jam dari Excel, atau teks: `08:30`, `8:30`, `08:30:00`. Angka pecahan dari Excel (misalnya `0.3368`) juga dibaca sebagai jam dan dibulatkan ke menit terdekat. |
| **Leave** | Sel centang (TRUE/FALSE), atau teks: `Yes`, `Y`, `True`, `1`, `Leave`, `Cuti`, `Izin` dianggap **Yes**; `No`, `N`, `False`, `0`, `-` dianggap **No**. |

### Contoh tabel Excel

| ID | Nama | Date | Attendance IN | Attendance OUT | Leave |
|---|---|---|---|---|---|
| EMP001 | Budi Santoso | 2026-09-24 | 08:30 | 17:00 | No |
| EMP002 | Siti Aminah | 2026-09-24 | 08:15 | 16:45 | No |
| EMP003 | Andi Wijaya | 2026-09-24 | | | Yes |

### Yang terjadi setelah upload

1. Isi file ditampilkan di tabel halaman **Attendance File**.
2. **Belum tersimpan di database** pada tahap ini — masih bisa Anda edit dulu.
3. Klik **Save** untuk menyimpan ke SQL Server.

---

## 9. Alur data aplikasi (flow)

### A. Alur upload Excel (dari file sampai tampil di layar)

```
File Excel (.xlsx)
   ↓
ClosedXML  →  ExcelAttendanceReader.Read(stream)
   ↓
ExcelAttendanceResult
   ├─ Rows   : List<AttendanceRowDto>   (baris yang valid)
   └─ Errors : List<string>             (baris yang dilewati + alasannya)
   ↓
AttendanceController.Upload(...)
   ↓
Razor View : Views/Attendance/Index.cshtml
   (tabel yang bisa diedit: Attendance IN / OUT / Leave)
```

### B. Alur simpan data (dari tombol Save sampai masuk database)

```
Razor View (form "Save")
   ↓
AttendanceController.Save(List<AttendanceRowDto> rows)
   ↓
AttendanceService.SaveAsync(rows)
   ↓
ApplicationDbContext (EF Core)
   ↓
Database SQL Server  →  tabel "Attendances"
   (ID + tanggal yang sama = di-update, bukan dibuat dobel)
```

### C. Alur menampilkan data tersimpan (tombol Load)

```
Input tanggal (yyyy-MM-dd)
   ↓
AttendanceController.Index(date)
   ↓
AttendanceService.GetByDateAsync(date)
   ↓
ApplicationDbContext (EF Core)
   ↓
SQL Server  →  tabel "Attendances"
   ↓
Razor View : daftar absensi pada tanggal tersebut
```

### D. Alur login Google

```
Klik "Sign in with Google"
   ↓
AccountController.GoogleLogin
   ↓
Google OAuth  →  pengguna memilih akun & menyetujui
   ↓
AccountController.GoogleResponse (/signin-google)
   ↓
Cookie Authentication (cookie "AttendanceApp.Auth")
   ↓
Halaman Home (sudah login)
```

### Ringkasan sederhana

```
Controller
    ↓
AttendanceService
    ↓
ApplicationDbContext
    ↓
SQL Server
```

Penjelasan per lapisan:

| Lapisan | Tugasnya | File |
|---|---|---|
| **Controller** | Menerima permintaan dari browser, memvalidasi input, memilih halaman yang ditampilkan. | `Controllers/*.cs` |
| **Service** | Berisi aturan bisnis: membaca Excel, menyimpan/mengambil absensi. | `Services/*.cs` |
| **DbContext** | Menerjemahkan objek C# menjadi perintah SQL (lewat EF Core). | `Data/ApplicationDBContext.cs` |
| **SQL Server** | Menyimpan data secara permanen. | tabel `Attendances` |

---

## 10. Kalau ada error (troubleshooting)

| Gejala / pesan error | Penyebab biasanya | Cara memperbaiki |
|---|---|---|
| `'dotnet' is not recognized` | .NET SDK belum terinstall atau terminal belum di-restart | Install .NET 10 SDK, lalu **tutup dan buka ulang** terminal. |
| `NETSDK1045: The current .NET SDK does not support targeting .NET 10.0` | SDK yang terinstall masih versi lama (misalnya .NET 8/9) | Install .NET 10 SDK. Cek dengan `dotnet --list-sdks`. |
| `'dotnet-ef' is not recognized` | Tool EF belum terinstall atau PATH belum berisi folder tool | `dotnet tool install --global dotnet-ef`, lalu restart terminal. Folder tool default: `%USERPROFILE%\.dotnet\tools`. |
| `A network-related or instance-specific error occurred...` | Service SQL Server tidak jalan, atau nama `Server=` salah | Jalankan service SQL Server (`services.msc`), lalu sesuaikan nama server di `appsettings.json`. |
| `Login failed for user '...'` | Akun Windows Anda tidak punya akses ke SQL Server | Berikan akses login pada SQL Server, atau pakai connection string dengan `User Id` + `Password`. |
| `Cannot open database "AttendanceDb" requested by the login` | Database belum dibuat | Jalankan `dotnet ef database update`. |
| `Failed to bind to address http://127.0.0.1:5168: address already in use` | Port sudah dipakai — biasanya karena aplikasi ini **masih berjalan** di terminal/VS Code/jendela lain | Tutup aplikasi yang masih jalan, cek pemakai port dengan `netstat -ano | findstr :5168`, lalu hentikan prosesnya (`taskkill /PID <nomor-pid> /F`). Atau jalankan dengan port lain: `dotnet run --urls "http://localhost:5199"`. |
| Browser menampilkan “Not secure / tidak aman” | Sertifikat HTTPS development belum dipercaya | `dotnet dev-certs https --trust`. |
| Tombol Google hanya menampilkan *"Login dengan Google belum dikonfigurasi..."* | `ClientId`/`ClientSecret` belum diisi | Isi lewat `dotnet user-secrets set ...`, lalu restart aplikasi. |
| `Error 400: redirect_uri_mismatch` saat login Google | Alamat yang dibuka di browser tidak sama dengan redirect URI yang didaftarkan di Google (misalnya membuka `http://localhost:5168` tetapi yang didaftarkan hanya `https://localhost:7275`) | Daftarkan **kedua** alamat: `http://localhost:5168/signin-google` dan `https://localhost:7275/signin-google` (tanpa garis miring di akhir). |
| Upload gagal: `only .xlsx files are accepted` | File masih `.xls`, `.csv`, atau ekstensi lain | Simpan ulang file sebagai **Excel Workbook (.xlsx)**. |
| `Missing required column(s): Nama, Date` | Judul kolom wajib tidak ditemukan di baris pertama | Pastikan ada kolom `ID`, `Nama`, `Date` (boleh memakai alias seperti `Name`/`Tanggal`). |
| Notifikasi kuning: `Row 7: Time '8.30' is not a valid time` | Format jam di Excel tidak dikenali | Ubah nilai jam menjadi format `HH:mm` (contoh `08:30`), atau gunakan sel bertipe jam di Excel. |
| Notifikasi merah saat Save: `Some rows are invalid -> ...` | Ada isian yang tidak sesuai aturan (misalnya ID kosong atau jam lebih dari 24) | Perbaiki baris yang disebutkan, lalu klik Save lagi. |
| Data tidak muncul padahal sudah Save | Tanggal yang dimuat belum sama dengan tanggal data | Di panel **3. Saved data (load from SQL Server)**, pilih tanggal sesuai data lalu klik **Load**. |
| Ingin mengulang setup database dari nol | Database ingin dibersihkan | Hapus database `AttendanceDb` di SSMS, lalu jalankan `dotnet ef database update` lagi. |

### Perintah bantu untuk mengecek kondisi project

```bash
// Versi SDK yang terpasang
dotnet --list-sdks

// Versi tool EF
dotnet ef --version

// Cek error khusus database / migration
dotnet ef migrations list

// Build untuk memastikan tidak ada error kode
dotnet build
```

---

## 11. Catatan penting

1. **Jangan simpan Client Secret di `appsettings.json`.**
   File itu ikut ter-upload ke GitHub. Gunakan `dotnet user-secrets set ...` (development) atau environment variable (server).
   `appsettings.json` sengaja dibiarkan kosong:
   ```json
   "Authentication": {
     "Google": { "ClientId": "", "ClientSecret": "" }
   }
   ```

2. **Migration `InitialCreate` tidak perlu dibuat ulang.**
   `dotnet ef migrations add InitialCreate` hanya dijalankan sekali saat project pertama kali dibuat. Untuk orang yang clone, cukup `dotnet ef database update`.

3. **Halaman yang butuh login.**
   `HomeController` dan `AttendanceController` memakai `[Authorize]`. Kalau belum login, Anda akan otomatis dibawa ke halaman `Account/Login`.

4. **Halaman pertama aplikasi adalah Login.**
   Route default di `Program.cs` diarahkan ke `{controller=Account}/{action=Login}`.

5. **Semua akun Google bisa login.**
   Saat ini belum ada pembatasan email/domain, dan belum ada peran (role) admin biasa. Kalau nanti perlu, pembatasan bisa ditambahkan di `AccountController.GoogleResponse` (misalnya memeriksa klaim email).

6. **Keunikan ID + tanggal dijaga oleh aplikasi, bukan oleh database.**
   Tabel `Attendances` tidak memiliki unique index untuk kombinasi `EmployeeId` + `AttendanceDate`. Aturan "data yang sama diperbarui, bukan digandakan" diterapkan di `AttendanceService.SaveAsync`. Kalau nanti data bertambah besar, menambahkan unique index bisa jadi perbaikan yang baik.

7. **File yang tidak perlu ikut ke GitHub.**
   `bin/`, `obj/`, `.vs/`, `.env` sudah masuk `.gitignore`. Kalau ada masalah "file build ikut ter-commit", pastikan `.gitignore` tidak dihapus.

---

## Lisensi & kontribusi

Project ini bersifat internal untuk keperluan absensi. Silakan tambahkan catatan lisensi bila akan dibagikan ke publik.

Kalau menemukan bug atau ingin menambah fitur:

1. Buat branch baru, contoh: `git checkout -b fitur-nama-baru`
2. Lakukan perubahan, lalu commit: `git commit -m "feat: tambah fitur ..."`
3. Kirim ke GitHub dan buat Pull Request.

