docker build -t firenza/repository_analytics_api:latest .
docker image prune -f
docker push firenza/repository_analytics_api:latest
pause
exit
