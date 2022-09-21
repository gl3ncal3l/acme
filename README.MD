## Devel Systems - EXAMEN PRACTICO NET CORE   

Agradezco que me hayan tomado en cuenta para realizar esta prueba. A continuación, se listan las instrucciones para el uso de la aplicación.

Instrucciones:  
1. Ejecutar el script ACMEDB.sql  
2. Modificar la cadena de conexión de la base de datos que se encuentra en el archivo *appsettings.json* para evitar error de conexión al momento de realizar las pruebas.
3. Realizar la importación del archivo *ACMECollection.postman_collection.json* en postman.
4. Para consultar las solicitudes primero se debe actualizar el token de autorización en las solicitudes que requieren autenticación, esto se hace por medio del endpoint */acme/JWT* el cual devolverá un token valido con vigencia de una hora.
