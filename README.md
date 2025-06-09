
# AuthService (.NET 8)

## Запуск
```bash
mkdir -p keys
openssl genpkey -algorithm RSA -out keys/private_key.pem -pkeyopt rsa_keygen_bits:2048
openssl rsa -in keys/private_key.pem -pubout -out keys/public_key.pem

docker-compose up --build
```

API: http://localhost:5000/swagger  
JWT RS256, refresh cookie (HttpOnly).

* Domain / Application / Infrastructure / Api = чистая модульность.
