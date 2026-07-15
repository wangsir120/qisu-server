@echo off
echo ============================================
echo   创建 review_replies 表
echo ============================================
echo.

sqlcmd -S localhost -U sa -P 123456 -d QisuDB -i "c:\Users\王SIR\Desktop\栖宿\qisu-server\Data\Migrations\CreateReviewRepliesTable.sql"

echo.
echo ============================================
echo   执行完成！请检查上面的输出结果
echo ============================================
pause
