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
- [1. Cara membuat project dari nol](#1-cara-membuat-project-dari-nol)
- [2. Cara menjalankan dari hasil clone GitHub](#2-cara-menjalankan-dari-hasil-clone-github)
- [3. Setup database (migration)](#3-setup-database-migration)
- [4. Cara memakai aplikasi](#4-cara-memakai-aplikasi)
- [5. Alur data aplikasi (flow)](#5-alur-data-aplikasi-flow)

---

## Fitur

| Fitur                    | Keterangan                                                                                                                                                                                        |
| ------------------------ | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Login Google             | Masuk aplikasi memakai akun Google (OAuth 2.0). Tidak ada form username/password.                                                                                                                 |
| Logout                   | Keluar dari aplikasi dan menghapus sesi login (alamat `/Account/Logout`).                                                                                                                         |
| Upload Excel             | Upload file absensi berformat `.xlsx`, lalu isinya dibaca otomatis.                                                                                                                               |
| Edit data sebelum simpan | Jam masuk (Attendance IN), jam keluar (Attendance OUT), dan pilihan Leave bisa diubah langsung di tabel. Employee ID dan Nama hanya bisa dilihat (readonly), karena dianggap data induk karyawan. |
| Hapus baris              | Baris yang tidak ingin disimpan bisa dihapus di tabel sebelum klik Save.                                                                                                                          |
| Simpan ke database       | Data disimpan ke SQL Server. Kalau kombinasi **ID + tanggal** sudah ada, datanya **diperbarui**, bukan dibuat dobel.                                                                              |
| Lihat data tersimpan     | Pilih tanggal lalu klik **Load** untuk memastikan data benar-benar sudah masuk database.                                                                                                          |
| Laporan baris bermasalah | Baris Excel yang datanya tidak valid tidak menggagalkan seluruh file — baris itu dilewati dan dilaporkan.                                                                                         |
| Halaman Access Denied    | Halaman yang muncul saat pengguna tidak punya izin mengakses halaman tertentu.                                                                                                                    |

---

## Tech stack (teknologi yang dipakai)

| Teknologi                                      | Versi                    | Dipakai untuk                                                                     |
| ---------------------------------------------- | ------------------------ | --------------------------------------------------------------------------------- |
| **.NET / C#**                                  | .NET 10 (`net10.0`)      | Bahasa dan platform utama aplikasi.                                               |
| **ASP.NET Core MVC**                           | 10                       | Kerangka kerja web: mengatur halaman (View), alur (Controller), dan data (Model). |
| **Razor View**                                 | —                        | File `.cshtml`, yaitu tampilan HTML yang bisa disisipi kode C#.                   |
| **Entity Framework Core** (SqlServer)          | `10.*`                   | Menghubungkan aplikasi C# ke database tanpa menulis query SQL manual.             |
| **Entity Framework Core Design**               | `10.*`                   | Alat bantu untuk membuat _migration_ (pembuat/pengubah struktur tabel).           |
| **SQL Server**                                 | —                        | Database tempat data absensi disimpan (tabel `Attendances`).                      |
| **ClosedXML**                                  | `0.105.1`                | Membaca file Excel (`.xlsx`) dari dalam kode C#.                                  |
| **Microsoft.AspNetCore.Authentication.Google** | `10.0.12`                | Login memakai akun Google (Google OAuth).                                         |
| **Cookie Authentication**                      | bawaan ASP.NET Core      | Menyimpan status login pengguna setelah berhasil login Google.                    |
| **Bootstrap**                                  | 5 (folder `wwwroot/lib`) | Membuat tampilan rapi dan responsif tanpa menulis banyak CSS.                     |
| **jQuery + jquery-validation**                 | (folder `wwwroot/lib`)   | Dipakai oleh validasi form bawaan ASP.NET Core dan skrip halaman.                 |
| **JavaScript (vanilla)**                       | —                        | Menambah/menghapus baris tabel dan menomori ulang nama field sebelum Save.        |
| **dotnet-ef CLI**                              | 10.x                     | Perintah terminal untuk membuat migration dan mengirimkannya ke database.         |

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

| Alat                                      | Kenapa butuh                               | Catatan                                                                              |
| ----------------------------------------- | ------------------------------------------ | ------------------------------------------------------------------------------------ |
| **.NET 10 SDK**                           | Untuk menjalankan dan mem-build aplikasi   | Wajib. Project ini memakai `net10.0`.                                                |
| **SQL Server**                            | Tempat data absensi disimpan               | SQL Server Express (gratis) sudah cukup, atau SQL Server versi lain yang Anda punya. |
| **Git**                                   | Untuk mengambil (clone) kode project       | Hanya perlu kalau mengambil dari GitHub.                                             |
| **Visual Studio Code atau Visual Studio** | Editor/IDE untuk melihat dan mengedit kode | Opsional, tapi sangat membantu.                                                      |
| **SQL Server Management Studio (SSMS)**   | Melihat isi tabel langsung di database     | Opsional.                                                                            |

---

## 1. Cara membuat project dari nol

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

> Di project ini langkah 6 **tidak perlu dijalankan lagi**, karena file migration `InitialCreate` sudah ada di folder `Migrations/`. Lihat penjelasan di [bagian 3](#3-setup-database-migration).

---

## 2. Cara menjalankan dari hasil clone GitHub

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

```bash
// melihat daftar secret yang tersimpan
dotnet user-secrets list

// menghapus satu secret
dotnet user-secrets remove "Authentication:Google:ClientSecret"
```

---

## 3. Setup database (migration)

- `dotnet ef migrations add <Nama>` → **membuat file catatan perubahan** (belum menyentuh database).
- `dotnet ef database update` → **menjalankan catatan itu ke database** (baru di sini tabel benar-benar dibuat/diubah).

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

```
Server=localhost\SQLEXPRESS;Database=AttendanceDb;User Id=sa;Password=PASSWORD_ANDA;TrustServerCertificate=True;
```

---

## 4. Cara memakai aplikasi

Berikut alur pemakaian dari sisi pengguna (tanpa perlu paham koding).

### Langkah 1 — Login

1. Buka alamat aplikasi yang tertulis di terminal — secara default `http://localhost:5168`.
2. Klik tombol **Sign in with Google**.
3. Pilih akun Google Anda → klik **Continue/Allow**.
4. Kalau berhasil, Anda akan diarahkan ke halaman **Home**.

Kalau tombol hanya menampilkan pesan _"Login dengan Google belum dikonfigurasi..."_, berarti langkah di [bagian 2](#2-cara-menjalankan-dari-hasil-clone-github) belum dijalankan.

### Langkah 2 — Buka menu Attendance File

Klik **Attendance File** di navbar atas.

Halaman ini terbagi menjadi beberapa bagian:

| Bagian                                   | Fungsi                                                                                                      |
| ---------------------------------------- | ----------------------------------------------------------------------------------------------------------- |
| **1. Upload Excel**                      | Pilih file `.xlsx` lalu klik **Upload** untuk menampilkan isinya di tabel.                                  |
| **2. Attendance Data**                   | Tabel yang berisi hasil upload dan bisa diedit.                                                             |
| **3. Saved data (load from SQL Server)** | Panel lipat (harus diklik dulu untuk membuka) yang memuat data tersimpan dari database berdasarkan tanggal. |

### Langkah 3 — Upload file Excel

1. Klik **Choose File / Pilih File**, pilih file absensi Anda (harus berformat `.xlsx`).
2. Klik **Upload**.
3. Kalau berhasil muncul notifikasi hijau, contoh: _"12 row(s) loaded from 'absensi-oktober.xlsx'..."_
4. Kalau ada baris yang datanya salah, muncul notifikasi kuning berisi daftar kesalahannya. Baris yang salah **tidak** ikut masuk tabel; baris yang benar tetap masuk.

### Langkah 4 — Edit data di tabel

Di tabel Anda bisa:

- Mengubah **Attendance IN** (jam masuk) — memakai pemilih waktu format `HH:mm`, contoh `08:30`.
- Mengubah **Attendance OUT** (jam keluar) — memakai pemilih waktu format `HH:mm`, contoh `17:00`.
- Mengubah **Leave** — pilih **Yes** (cuti/izin) atau **No** dari daftar pilihan.
- Menghapus baris yang tidak perlu disimpan, memakai tombol **Remove** di ujung kanan baris.
- Melihat jumlah baris di badge di sebelah kanan judul tabel.

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

---

## 5. Alur data aplikasi (flow)

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

| Lapisan        | Tugasnya                                                                               | File                           |
| -------------- | -------------------------------------------------------------------------------------- | ------------------------------ |
| **Controller** | Menerima permintaan dari browser, memvalidasi input, memilih halaman yang ditampilkan. | `Controllers/*.cs`             |
| **Service**    | Berisi aturan bisnis: membaca Excel, menyimpan/mengambil absensi.                      | `Services/*.cs`                |
| **DbContext**  | Menerjemahkan objek C# menjadi perintah SQL (lewat EF Core).                           | `Data/ApplicationDBContext.cs` |
| **SQL Server** | Menyimpan data secara permanen.                                                        | tabel `Attendances`            |
