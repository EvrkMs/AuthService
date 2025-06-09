
# AuthService (.NET 8)

## Запуск
```bash
docker-compose up --build
```

API: http://localhost:5000/swagger  
JWT RS256 (ключи генерируются при старте), refresh cookie (HttpOnly).

* Domain / Application / Infrastructure / Api = чистая модульность.
