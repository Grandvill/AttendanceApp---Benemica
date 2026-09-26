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
2. Pilih **SDK** (bukan Runtime) untuk Windows x64, lalu unduh dan jalankan installer-nya (klik _Next/Install_ sampai selesai).
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

## 5. Setup login Google (OAuth)

Aplikasi ini tidak punya form username/password — satu-satunya cara login adalah lewat akun Google.

### Menyimpan kredensial ke project

Di folder project, jalankan:

```bash
dotnet user-secrets set "Authentication:Google:ClientId" "ISI_CLIENT_ID_ANDA"
dotnet user-secrets set "Authentication:Google:ClientSecret" "ISI_CLIENT_SECRET_ANDA"
```

Kenapa pakai `user-secrets`? Supaya Client Secret **tidak ikut ter-upload ke GitHub**.

Perintah bantu lainnya:

```bash
// melihat daftar secret yang tersimpan
dotnet user-secrets list

// menghapus satu secret
dotnet user-secrets remove "Authentication:Google:ClientSecret"
```

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

| Profil               | Alamat                                               | Cara menjalankan                    |
| -------------------- | ---------------------------------------------------- | ----------------------------------- |
| `http` (**default**) | `http://localhost:5168`                              | `dotnet run`                        |
| `https`              | `https://localhost:7275` dan `http://localhost:5168` | `dotnet run --launch-profile https` |

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

Kalau tombol hanya menampilkan pesan _"Login dengan Google belum dikonfigurasi..."_, berarti langkah di [bagian 5](#5-setup-login-google-oauth) belum dijalankan.

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

| Kolom              | Wajib?       | Isi                                        | Contoh                |
| ------------------ | ------------ | ------------------------------------------ | --------------------- |
| **ID**             | **Wajib**    | ID karyawan. Boleh berupa teks atau angka. | `EMP001` atau `10001` |
| **Nama**           | **Wajib**    | Nama karyawan.                             | `Budi Santoso`        |
| **Date**           | **Wajib**    | Tanggal absensi.                           | `2026-09-24`          |
| **Attendance IN**  | Boleh kosong | Jam masuk.                                 | `08:30`               |
| **Attendance OUT** | Boleh kosong | Jam keluar.                                | `17:00`               |
| **Leave**          | Boleh kosong | Status cuti/izin.                          | `No` atau `Yes`       |

### Judul kolom yang juga diterima (alias)

Aplikasi mengenali beberapa penulisan judul berikut, jadi Anda tidak wajib menulis persis sama:

| Kolom          | Judul yang diterima                                                    |
| -------------- | ---------------------------------------------------------------------- |
| ID             | `ID`, `EmployeeId`, `Employee`, `EmployeeCode`, `NIK`                  |
| Nama           | `Nama`, `Name`, `EmployeeName`, `NamaKaryawan`                         |
| Date           | `Date`, `Tanggal`, `AttendanceDate`                                    |
| Attendance IN  | `AttendanceIn`, `In`, `TimeIn`, `CheckIn`, `ClockIn`, `JamMasuk`       |
| Attendance OUT | `AttendanceOut`, `Out`, `TimeOut`, `CheckOut`, `ClockOut`, `JamKeluar` |
| Leave          | `Leave`, `IsLeave`, `Cuti`, `Izin`                                     |

### Format tanggal & jam yang diterima

| Data                    | Format yang diterima                                                                                                                                                       |
| ----------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **Date**                | Bisa berupa sel bertipe tanggal asli Excel, atau teks: `yyyy-MM-dd` (disarankan), `yyyy/MM/dd`, `dd/MM/yyyy`, `dd-MM-yyyy`. Contoh: `2026-09-24`.                          |
| **Attendance IN / OUT** | Sel bertipe jam dari Excel, atau teks: `08:30`, `8:30`, `08:30:00`. Angka pecahan dari Excel (misalnya `0.3368`) juga dibaca sebagai jam dan dibulatkan ke menit terdekat. |
| **Leave**               | Sel centang (TRUE/FALSE), atau teks: `Yes`, `Y`, `True`, `1`, `Leave`, `Cuti`, `Izin` dianggap **Yes**; `No`, `N`, `False`, `0`, `-` dianggap **No**.                      |

### Contoh tabel Excel

| ID     | Nama         | Date       | Attendance IN | Attendance OUT | Leave |
| ------ | ------------ | ---------- | ------------- | -------------- | ----- |
| EMP001 | Budi Santoso | 2026-09-24 | 08:30         | 17:00          | No    |
| EMP002 | Siti Aminah  | 2026-09-24 | 08:15         | 16:45          | No    |
| EMP003 | Andi Wijaya  | 2026-09-24 |               |                | Yes   |

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

| Lapisan        | Tugasnya                                                                               | File                           |
| -------------- | -------------------------------------------------------------------------------------- | ------------------------------ |
| **Controller** | Menerima permintaan dari browser, memvalidasi input, memilih halaman yang ditampilkan. | `Controllers/*.cs`             |
| **Service**    | Berisi aturan bisnis: membaca Excel, menyimpan/mengambil absensi.                      | `Services/*.cs`                |
| **DbContext**  | Menerjemahkan objek C# menjadi perintah SQL (lewat EF Core).                           | `Data/ApplicationDBContext.cs` |
| **SQL Server** | Menyimpan data secara permanen.                                                        | tabel `Attendances`            |

---
