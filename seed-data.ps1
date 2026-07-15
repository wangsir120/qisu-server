$server = "localhost"
$user = "sa"
$pass = "123456"
$db = "QisuDB"
$today = (Get-Date).ToString("yyyy-MM-dd")
$hostId = 7
$uid = 10000001

function Run-Sql($sql) {
    sqlcmd -S $server -U $user -P $pass -d $db -Q $sql | Out-Null
}

# 1. 待确认订单
Run-Sql "INSERT INTO Orders(order_no,user_id,property_id,host_id,check_in_date,check_out_date,nights,guest_count,guest_name,guest_phone,price_per_night,subtotal,cleaning_fee,service_fee,total_price,status,payment_method,created_at,updated_at) VALUES('TST-P001',$uid,113,$hostId,'2026-05-22','2026-05-25',3,2,N'张三','13800001001',348.00,1044.00,50.00,109.40,1203.40,'pending','wechat',GETDATE(),GETDATE())"
Run-Sql "INSERT INTO Orders(order_no,user_id,property_id,host_id,check_in_date,check_out_date,nights,guest_count,guest_name,guest_phone,price_per_night,subtotal,cleaning_fee,service_fee,total_price,status,payment_method,created_at,updated_at) VALUES('TST-P002',$uid,123,$hostId,'2026-05-20','2026-05-22',2,1,N'李四','13800001002',528.00,1056.00,80.00,113.60,1249.60,'pending','alipay',GETDATE(),GETDATE())"
Run-Sql "INSERT INTO Orders(order_no,user_id,property_id,host_id,check_in_date,check_out_date,nights,guest_count,guest_name,guest_phone,price_per_night,subtotal,cleaning_fee,service_fee,total_price,status,payment_method,created_at,updated_at) VALUES('TST-P003',$uid,211,$hostId,'2026-05-26','2026-05-29',3,3,N'王五','13800001003',358.00,1074.00,50.00,112.40,1236.40,'pending','wechat',GETDATE(),GETDATE())"

# 2. 今日入住
Run-Sql "INSERT INTO Orders(order_no,user_id,property_id,host_id,check_in_date,check_out_date,nights,guest_count,guest_name,guest_phone,price_per_night,subtotal,cleaning_fee,service_fee,total_price,status,payment_method,payment_time,created_at,updated_at) VALUES('TST-CI001',$uid,115,$hostId,'$today','2026-05-21',2,2,N'赵六','13900002001',228.00,456.00,30.00,48.60,534.60,'paid','wechat',DATEADD(hour,-2,GETDATE()),DATEADD(hour,-2,GETDATE()),GETDATE())"
Run-Sql "INSERT INTO Orders(order_no,user_id,property_id,host_id,check_in_date,check_out_date,nights,guest_count,guest_name,guest_phone,price_per_night,subtotal,cleaning_fee,service_fee,total_price,status,payment_method,payment_time,created_at,updated_at) VALUES('TST-CI002',$uid,118,$hostId,'$today','2026-05-22',3,1,N'孙七','13900002002',358.00,1074.00,50.00,112.40,1236.40,'paid','alipay',DATEADD(hour,-5,GETDATE()),DATEADD(hour,-5,GETDATE()),GETDATE())"

# 3. 今日退房
Run-Sql "INSERT INTO Orders(order_no,user_id,property_id,host_id,check_in_date,check_out_date,nights,guest_count,guest_name,guest_phone,price_per_night,subtotal,cleaning_fee,service_fee,total_price,status,payment_method,payment_time,created_at,updated_at) VALUES('TST-CO001',$uid,120,$hostId,'2026-05-16','$today',3,2,N'周八','13700003001',318.00,954.00,50.00,100.40,1104.40,'paid','wechat',DATEADD(day,-3,GETDATE()),DATEADD(day,-3,GETDATE()),GETDATE())"
Run-Sql "INSERT INTO Orders(order_no,user_id,property_id,host_id,check_in_date,check_out_date,nights,guest_count,guest_name,guest_phone,price_per_night,subtotal,cleaning_fee,service_fee,total_price,status,payment_method,payment_time,created_at,updated_at) VALUES('TST-CO002',$uid,134,$hostId,'2026-05-17','$today',2,1,N'吴九','13700003002',458.00,916.00,80.00,99.60,1095.60,'paid','alipay',DATEADD(day,-2,GETDATE()),DATEADD(day,-2,GETDATE()),GETDATE())"

# 4. 在住
Run-Sql "INSERT INTO Orders(order_no,user_id,property_id,host_id,check_in_date,check_out_date,nights,guest_count,guest_name,guest_phone,price_per_night,subtotal,cleaning_fee,service_fee,total_price,status,payment_method,payment_time,created_at,updated_at) VALUES('TST-ST001',$uid,126,$hostId,'2026-05-14','2026-05-21',7,2,N'郑十','13600004001',388.00,2716.00,80.00,279.60,3075.60,'staying','wechat',DATEADD(day,-5,GETDATE()),DATEADD(day,-5,GETDATE()),GETDATE())"
Run-Sql "INSERT INTO Orders(order_no,user_id,property_id,host_id,check_in_date,check_out_date,nights,guest_count,guest_name,guest_phone,price_per_night,subtotal,cleaning_fee,service_fee,total_price,status,payment_method,payment_time,created_at,updated_at) VALUES('TST-ST002',$uid,142,$hostId,'2026-05-17','2026-05-22',5,3,N'钱十一','13600004002',288.00,1440.00,50.00,149.00,1639.00,'staying','alipay',DATEADD(day,-2,GETDATE()),DATEADD(day,-2,GETDATE()),GETDATE())"
Run-Sql "INSERT INTO Orders(order_no,user_id,property_id,host_id,check_in_date,check_out_date,nights,guest_count,guest_name,guest_phone,price_per_night,subtotal,cleaning_fee,service_fee,total_price,status,payment_method,payment_time,created_at,updated_at) VALUES('TST-ST003',$uid,213,$hostId,'2026-05-18','2026-05-23',5,2,N'陈十二','13600004003',488.00,2440.00,100.00,254.00,2794.00,'staying','wechat',DATEADD(day,-1,GETDATE()),DATEADD(day,-1,GETDATE()),GETDATE())"

# 5. 今日营收
Run-Sql "INSERT INTO Orders(order_no,user_id,property_id,host_id,check_in_date,check_out_date,nights,guest_count,guest_name,guest_phone,price_per_night,subtotal,cleaning_fee,service_fee,total_price,status,payment_method,payment_time,created_at,updated_at) VALUES('TST-R001',$uid,145,$hostId,'2026-05-29','2026-06-01',3,2,N'林十三','13500005001',358.00,1074.00,50.00,112.40,1236.40,'paid','wechat',GETDATE(),GETDATE(),GETDATE())"
Run-Sql "INSERT INTO Orders(order_no,user_id,property_id,host_id,check_in_date,check_out_date,nights,guest_count,guest_name,guest_phone,price_per_night,subtotal,cleaning_fee,service_fee,total_price,status,payment_method,payment_time,created_at,updated_at) VALUES('TST-R002',$uid,148,$hostId,'2026-06-03','2026-06-05',2,1,N'黄十四','13500005002',328.00,656.00,30.00,68.60,754.60,'paid','alipay',DATEADD(minute,-30,GETDATE()),GETDATE(),GETDATE())"
Run-Sql "INSERT INTO Orders(order_no,user_id,property_id,host_id,check_in_date,check_out_date,nights,guest_count,guest_name,guest_phone,price_per_night,subtotal,cleaning_fee,service_fee,total_price,status,payment_method,payment_time,created_at,updated_at) VALUES('TST-R003',$uid,151,$hostId,'2026-06-08','2026-06-11',3,3,N'刘十五','13500005003',248.00,744.00,50.00,79.40,873.40,'paid','wechat',DATEADD(hour,-1,GETDATE()),GETDATE(),GETDATE())"

Write-Host "基础13条完成，开始趋势数据..."

for ($i = 1; $i -le 30; $i++) {
    $d = (Get-Date).AddDays(-$i).ToString("yyyy-MM-dd")
    $propId = 113 + (($i * 7 + 3) % 150)
    $nights = ($i % 3) + 1
    $price = [math]::Round(168.0 + (($propId % 10) * 30), 2)
    $subtotal = [math]::Round($price * $nights, 2)
    if ($price -ge 300) { $cleaningFee = 80.0 } else { $cleaningFee = 30.0 }
    $serviceFee = [math]::Round($subtotal * 0.05 + $cleaningFee * 0.03, 2)
    $totalPrice = [math]::Round($subtotal + $cleaningFee + $serviceFee, 2)
    $orderNo = "TRD$d".Replace("-","") + ($i.ToString("D3"))
    $pmtMethod = if ($i % 2 -eq 0) { "wechat" } else { "alipay" }
    
    if ($i % 15 -eq 0) {
        $status = "cancelled"
        $pmtSql = "NULL"
        $updSql = "DATEADD(day,1,'$d')"
    } else {
        $status = "completed"
        $pmtSql = "DATEADD(hour,12,'$d')"
        $updSql = "DATEADD(hour,13,'$d')"
    }

    Run-Sql "INSERT INTO Orders(order_no,user_id,property_id,host_id,check_in_date,check_out_date,nights,guest_count,guest_name,guest_phone,price_per_night,subtotal,cleaning_fee,service_fee,total_price,status,payment_method,payment_time,created_at,updated_at) VALUES('$orderNo',$uid,$propId,$hostId,'$d',(DATEADD(day,$nights,'$d')),$nights,(1+($i%3)),N'TEST$i','1380001'+RIGHT('00000'+($i.ToString()),5),$price,$subtotal,$cleaningFee,$serviceFee,$totalPrice,'$status','$pmtMethod',$pmtSql,'$d',$updSql)"

    if ($i % 2 -eq 0) {
        $propId2 = 200 + (($i * 11 + 5) % 120)
        $nights2 = (($i % 4) + 1)
        $price2 = [math]::Round(198.0 + (($propId2 % 8) * 35), 2)
        $subtotal2 = [math]::Round($price2 * $nights2, 2)
        if ($price2 -ge 350) { $cleaningFee2 = 80.0 } else { $cleaningFee2 = 30.0 }
        $serviceFee2 = [math]::Round($subtotal2 * 0.05 + $cleaningFee2 * 0.03, 2)
        $totalPrice2 = [math]::Round($subtotal2 + $cleaningFee2 + $serviceFee2, 2)
        $orderNo2 = "TRD$d".Replace("-","") + (($i+500).ToString("D3"))
        
        Run-Sql "INSERT INTO Orders(order_no,user_id,property_id,host_id,check_in_date,check_out_date,nights,guest_count,guest_name,guest_phone,price_per_night,subtotal,cleaning_fee,service_fee,total_price,status,payment_method,payment_time,created_at,updated_at) VALUES('$orderNo2',$uid,$propId2,$hostId,'$d',(DATEADD(day,$nights2,'$d')),$nights2,2,N'BK$i','1390002'+RIGHT('00000'+($i.ToString()),5),$price2,$subtotal2,$cleaningFee2,$serviceFee2,$totalPrice2,'completed','wechat',DATEADD(hour,14,'$d'),'$d',DATEADD(hour,15,'$d'))"
    }
}

Run-Sql "UPDATE Hosts SET total_listings=(SELECT COUNT(*) FROM Properties WHERE host_id=$hostId AND status=1), rating=4.7, total_reviews=12, updated_at=GETDATE() WHERE id=$hostId"

Write-Host "=== DONE ==="
