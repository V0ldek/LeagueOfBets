docker stack rm leagueofbets
docker-compose build
docker-compose push
docker stack deploy -c docker-compose.yml leagueofbets --with-registry-auth
read -n 1