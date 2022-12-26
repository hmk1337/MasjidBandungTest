# Masjid Bandung

## POST /stop
Menghentikan semua gerak motor

Respon:

```json
    {
        "status":"ok"
    }
```

## GET,POST /status
Cek status dan posisi motor

- 0 kontrol error       -> Error
- 1 diam ready home     -> Other
- 2 diam ready          -> Idle
- 3 sedang gerak        -> Busy

Respon:

```json
[
    {
        "id": 1,
        "state": "Idle",
        "position": 0
    },
    {
        "id": 2,
        "state": "Error"
        "position": 100,
    },
    {
        "id": 3,
        "state": "Idle",
        "position": 50
    },
    {
        "id": 4,
        "state": "Busy",
        "position": 90
    }
]
```

## POST /coreograpy/addsingle
Menambah perintah gerak ke antrean, motor tidak langsung gerak

```json
{
    "coreography":"nama",
    "position": [100,50,20,60],     // array of double
    "speed": 1000,                  // (optional) kecepatan gerak
    "time": 10,                     // (optional) waktu tempuh dalam detik (belum implementasi)
}

```

## POST 192.168.99.100:8080/coreograpy/addmultiple
Menambah perintah gerak ke antrean, motor tidak langsung gerak

```json
{
    "coreography":"nama",
    "position": [-1,50,20,60],     // array of double, 0-100, -1 = diam
    "color": [
        "#FFFF00",
        "Aquamarine",
        "#FF0000",
        ""
    ]
    "speed": 1000,                  // (optional) kecepatan gerak
    "time": 10,                     // (optional) waktu tempuh dalam detik (belum implementasi)
}

```

## POST /coreography/clear

## POST /coreography/check
{
    "" :
}

## POST /move
Menjalankan perintah berikutnya jika ada, motor langsung gerak

Respon:
- 200 jika berhasil
- 400 jika antrean belum diset

## POST /move/{id:string}
Contoh: 192.168.99.200:8080/move/sebuahnama
Menjalankan perintah dengan id tertentu, motor langsung gerak

Respon:
- 200 jika berhasil
- 400 jika antrean belum diset

## POST /manual
Memerintahkan motor untuk bergerak berdasarkan perintah yang dikirim


## POST /manualled
Mengatur warna LED

```json
{
    "color": [200,200,100]
}
```

## POST /home
Reset posisi ke home

## POST /settings
Mengatur parameter (belum implementasi)

```json
{
    "travelDistance": 40, // cm
    "maxSpeed": 3000  // mm/min
}
```

## GET,POST /info
Mendapatkan informasi sistem dan kontroler

Respon

```json
{
    "serialNo":"123-333",
}
```
