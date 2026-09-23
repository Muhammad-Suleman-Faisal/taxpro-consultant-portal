# TaxPro Consultant Portal - Native Web Server & API Backend
# Zero-dependency HTTP server running via .NET System.Net.HttpListener

param(
    [int]$Port = 3000
)

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
if (-not $scriptDir) { $scriptDir = (Get-Location).Path }

$dataDir = Join-Path $scriptDir "data"
if (-not (Test-Path $dataDir)) { New-Item -ItemType Directory -Path $dataDir | Out-Null }

$bookingsFile = Join-Path $dataDir "bookings.json"
if (-not (Test-Path $bookingsFile)) { "[]" | Out-File -FilePath $bookingsFile -Encoding utf8 }

$inquiriesFile = Join-Path $dataDir "inquiries.json"
if (-not (Test-Path $inquiriesFile)) { "[]" | Out-File -FilePath $inquiriesFile -Encoding utf8 }

$mimeTypes = @{
    ".html" = "text/html; charset=utf-8"
    ".css"  = "text/css; charset=utf-8"
    ".js"   = "application/javascript; charset=utf-8"
    ".json" = "application/json; charset=utf-8"
    ".svg"  = "image/svg+xml"
    ".png"  = "image/png"
    ".jpg"  = "image/jpeg"
    ".jpeg" = "image/jpeg"
    ".ico"  = "image/x-icon"
    ".txt"  = "text/plain; charset=utf-8"
}

$listener = New-Object System.Net.HttpListener
$listener.Prefixes.Add("http://localhost:$Port/")
$listener.Prefixes.Add("http://127.0.0.1:$Port/")

try {
    $listener.Start()
    Write-Host "==========================================================" -ForegroundColor Green
    Write-Host " TaxPro Consultant Portal Server LIVE" -ForegroundColor Cyan
    Write-Host " URL: http://localhost:$Port" -ForegroundColor Yellow
    Write-Host " Root: $scriptDir" -ForegroundColor Gray
    Write-Host " Press Ctrl+C in this terminal to stop the server." -ForegroundColor Gray
    Write-Host "==========================================================" -ForegroundColor Green
} catch {
    Write-Error "Failed to start listener on port $($Port): $_"
    exit 1
}

function Send-JsonResponse($response, $statusCode, $obj) {
    $json = $obj | ConvertTo-Json -Depth 5 -Compress
    $bytes = [System.Text.Encoding]::UTF8.GetBytes($json)
    $response.StatusCode = $statusCode
    $response.ContentType = "application/json; charset=utf-8"
    $response.AddHeader("Access-Control-Allow-Origin", "*")
    $response.AddHeader("Access-Control-Allow-Methods", "GET, POST, OPTIONS")
    $response.AddHeader("Access-Control-Allow-Headers", "Content-Type")
    $response.ContentLength64 = $bytes.Length
    $response.OutputStream.Write($bytes, 0, $bytes.Length)
    $response.OutputStream.Close()
}

function Send-FileResponse($response, $filePath) {
    $ext = [System.IO.Path]::GetExtension($filePath).ToLower()
    $mime = if ($mimeTypes.ContainsKey($ext)) { $mimeTypes[$ext] } else { "application/octet-stream" }
    
    $bytes = [System.IO.File]::ReadAllBytes($filePath)
    $response.StatusCode = 200
    $response.ContentType = $mime
    $response.AddHeader("Access-Control-Allow-Origin", "*")
    $response.ContentLength64 = $bytes.Length
    $response.OutputStream.Write($bytes, 0, $bytes.Length)
    $response.OutputStream.Close()
}

function Get-RequestBody($request) {
    $reader = New-Object System.IO.StreamReader($request.InputStream, $request.ContentEncoding)
    $body = $reader.ReadToEnd()
    $reader.Close()
    return $body
}

while ($listener.IsListening) {
    try {
        $context = $listener.GetContext()
        $request = $context.Request
        $response = $context.Response
        $urlPath = $request.Url.LocalPath
        $method = $request.HttpMethod

        # Handle CORS Preflight
        if ($method -eq "OPTIONS") {
            $response.StatusCode = 204
            $response.AddHeader("Access-Control-Allow-Origin", "*")
            $response.AddHeader("Access-Control-Allow-Methods", "GET, POST, OPTIONS")
            $response.AddHeader("Access-Control-Allow-Headers", "Content-Type")
            $response.OutputStream.Close()
            continue
        }

        # API: Tax Slabs
        if ($urlPath -eq "/api/tax-slabs" -and $method -eq "GET") {
            $slabs = @{
                taxYear = "2024-2025"
                statute = "Finance Act 2024 (Income Tax Ordinance 2001)"
                salaried = @(
                    @{ min = 0; max = 600000; rate = 0; base = 0 },
                    @{ min = 600000; max = 1200000; rate = 0.05; base = 0 },
                    @{ min = 1200000; max = 2200000; rate = 0.15; base = 30000 },
                    @{ min = 2200000; max = 3200000; rate = 0.25; base = 180000 },
                    @{ min = 3200000; max = 4100000; rate = 0.30; base = 430000 },
                    @{ min = 4100000; max = -1; rate = 0.35; base = 700000 }
                )
                business = @(
                    @{ min = 0; max = 600000; rate = 0; base = 0 },
                    @{ min = 600000; max = 1200000; rate = 0.15; base = 0 },
                    @{ min = 1200000; max = 1600000; rate = 0.20; base = 90000 },
                    @{ min = 1600000; max = 3200000; rate = 0.30; base = 170000 },
                    @{ min = 3200000; max = 5600000; rate = 0.40; base = 650000 },
                    @{ min = 5600000; max = -1; rate = 0.45; base = 1610000 }
                )
                corporateRate = 0.29
            }
            Send-JsonResponse $response 200 $slabs
            continue
        }

        # API: Bookings GET
        if ($urlPath -eq "/api/bookings" -and $method -eq "GET") {
            $content = Get-Content -Raw -Path $bookingsFile -Encoding utf8
            if (-not $content) { $content = "[]" }
            $data = $content | ConvertFrom-Json
            Send-JsonResponse $response 200 $data
            continue
        }

        # API: Bookings POST
        if ($urlPath -eq "/api/bookings" -and $method -eq "POST") {
            $body = Get-RequestBody $request
            $item = $body | ConvertFrom-Json
            
            $rnd = Get-Random -Minimum 10000 -Maximum 99999
            $bookingId = "TXP-$rnd"
            
            $newBooking = @{
                id = $bookingId
                service = $item.service
                date = $item.date
                time = $item.time
                name = $item.name
                company = $item.company
                phone = $item.phone
                status = "Confirmed"
                createdAt = (Get-Date).ToString("yyyy-MM-ddTHH:mm:ssZ")
            }

            $currentData = @()
            if (Test-Path $bookingsFile) {
                $raw = Get-Content -Raw -Path $bookingsFile -Encoding utf8
                if ($raw) { $currentData = @($raw | ConvertFrom-Json) }
            }
            $currentData += $newBooking
            $currentData | ConvertTo-Json -Depth 5 | Out-File -FilePath $bookingsFile -Encoding utf8
            
            Send-JsonResponse $response 201 @{
                success = $true
                message = "Consultation successfully scheduled."
                booking = $newBooking
            }
            continue
        }

        # API: Consultations POST (Advance Payment Required)
        if ($urlPath -eq "/api/consultations" -and $method -eq "POST") {
            $body = Get-RequestBody $request
            $item = $body | ConvertFrom-Json
            
            $rnd = Get-Random -Minimum 10000 -Maximum 99999
            $consultationId = "TXP-ADV-$rnd"
            
            $newConsultation = @{
                id = $consultationId
                mode = $item.mode
                consultationType = $item.consultationType
                description = $item.description
                date = $item.date
                time = $item.time
                name = $item.name
                phone = $item.phone
                email = $item.email
                company = $item.company
                payment = @{
                    amount = $item.payment.amount
                    method = $item.payment.method
                    status = "Paid"
                    transactionRef = $item.payment.transactionRef
                    paidAt = (Get-Date).ToString("yyyy-MM-ddTHH:mm:ssZ")
                }
                status = "Paid & Confirmed"
                privacyNotice = if ($item.mode -eq "face_to_face") { "Private meeting details are shared only with confirmed clients." } else { "Encrypted video link dispatched via private channel." }
                createdAt = (Get-Date).ToString("yyyy-MM-ddTHH:mm:ssZ")
            }

            $consultationsFile = Join-Path $dataDir "consultations.json"
            $currentData = @()
            if (Test-Path $consultationsFile) {
                $raw = Get-Content -Raw -Path $consultationsFile -Encoding utf8
                if ($raw) { $currentData = @($raw | ConvertFrom-Json) }
            }
            $currentData += $newConsultation
            $currentData | ConvertTo-Json -Depth 5 | Out-File -FilePath $consultationsFile -Encoding utf8
            
            Send-JsonResponse $response 201 @{
                success = $true
                message = "Consultation confirmed after advance payment."
                consultation = $newConsultation
            }
            continue
        }

        # API: Inquiries POST
        if ($urlPath -eq "/api/inquiries" -and $method -eq "POST") {
            $body = Get-RequestBody $request
            $item = $body | ConvertFrom-Json

            $rnd = Get-Random -Minimum 10000 -Maximum 99999
            $inquiryId = "INQ-$rnd"

            $newInquiry = @{
                id = $inquiryId
                name = $item.name
                email = $item.email
                phone = $item.phone
                bizType = $item.bizType
                service = $item.service
                message = $item.message
                createdAt = (Get-Date).ToString("yyyy-MM-ddTHH:mm:ssZ")
            }

            $currentData = @()
            if (Test-Path $inquiriesFile) {
                $raw = Get-Content -Raw -Path $inquiriesFile -Encoding utf8
                if ($raw) { $currentData = @($raw | ConvertFrom-Json) }
            }
            $currentData += $newInquiry
            $currentData | ConvertTo-Json -Depth 5 | Out-File -FilePath $inquiriesFile -Encoding utf8

            Send-JsonResponse $response 201 @{
                success = $true
                message = "Official inquiry recorded."
                inquiry = $newInquiry
            }
            continue
        }

        # Static File Serving
        $localPath = $urlPath.TrimStart('/')
        if (-not $localPath) { $localPath = "index.html" }
        $fullPath = Join-Path $scriptDir $localPath

        if (Test-Path $fullPath -PathType Leaf) {
            Send-FileResponse $response $fullPath
        } else {
            $response.StatusCode = 404
            $msg = [System.Text.Encoding]::UTF8.GetBytes("404 Not Found")
            $response.ContentType = "text/plain"
            $response.OutputStream.Write($msg, 0, $msg.Length)
            $response.OutputStream.Close()
        }
    } catch {
        Write-Warning "Request error: $_"
    }
}
