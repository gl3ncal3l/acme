
CREATE DATABASE acmedb;
GO

USE acmedb;
GO

CREATE TABLE encuestas(
	id_encuesta INT NOT NULL PRIMARY KEY IDENTITY(1,1),
	nombre VARCHAR(255) NOT NULL,
	descripcion VARCHAR(255) NOT NULL,
	link VARCHAR(255) NOT NULL
);

CREATE TABLE campos(
	id_campo INT NOT NULL PRIMARY KEY IDENTITY(1,1),
	nombre VARCHAR(255) NOT NULL,
	titulo VARCHAR(255) NOT NULL,
	requerido VARCHAR(1) NOT NULL,
	tipo VARCHAR(25) NOT NULL,
	id_encuesta INT NOT NULL FOREIGN KEY REFERENCES encuestas(id_encuesta) ON DELETE CASCADE
);


CREATE TABLE respuestas(
	id_respuesta INT NOT NULL PRIMARY KEY IDENTITY(1,1),
	respuesta VARCHAR(255),
	id_campo INT NOT NULL FOREIGN KEY REFERENCES campos(id_campo) ON DELETE CASCADE
);
GO


CREATE PROCEDURE agregarEncuesta
	@nombre VARCHAR(255),
	@descripcion varchar(255),
	@link varchar(255)
AS
BEGIN
	INSERT INTO encuestas
	VALUES(@nombre,@descripcion,@link);

	SELECT TOP 1 e.id_encuesta
	FROM encuestas e
	ORDER BY e.id_encuesta DESC;

END
GO


CREATE PROCEDURE eliminarEncuesta
	@id_encuesta INT
AS
DECLARE @error_message nvarchar(255)
BEGIN
	IF EXISTS(SELECT * FROM encuestas WHERE id_encuesta = @id_encuesta)
		BEGIN
			DELETE FROM encuestas
			WHERE encuestas.id_encuesta = @id_encuesta;
		END
	ELSE
		BEGIN
			SET @error_message ='La encuesta no existe!'
			RAISERROR (@error_message, 10, 1) 
		END
END
GO


CREATE PROCEDURE modificarEncuesta
	@id_encuesta INT,
	@nombre VARCHAR(255),
	@descripcion varchar(255),
	@link varchar(255)
AS
DECLARE @error_message nvarchar(255)
BEGIN

	IF EXISTS(SELECT * FROM encuestas WHERE id_encuesta = @id_encuesta)
		BEGIN
			
			DELETE respuestas  
			FROM respuestas r 
			INNER JOIN campos c ON c.id_campo = r.id_campo
			WHERE c.id_encuesta = @id_encuesta;

			DELETE campos
			FROM campos c
			INNER JOIN encuestas e ON e.id_encuesta = c.id_encuesta
			WHERE e.id_encuesta = @id_encuesta;

			UPDATE encuestas
			SET nombre = @nombre, descripcion = @descripcion, link = @link
			WHERE id_encuesta = @id_encuesta

		END
	ELSE
		BEGIN
			SET @error_message ='La encuesta no existe!'
			RAISERROR (@error_message, 10, 1) 
		END

END
GO

CREATE PROCEDURE agregarCampo
	@nombre VARCHAR(255),
	@titulo VARCHAR(255),
	@requerido VARCHAR(1),
	@tipo VARCHAR(25),
	@id_encuesta INT 
AS
BEGIN
	INSERT INTO campos
	VALUES(@nombre,@titulo,@requerido, @tipo, @id_encuesta);
END
GO


CREATE PROCEDURE obtenerEncuesta
	@id_encuesta INT 
AS
BEGIN
	SELECT *
	FROM campos c
	WHERE c.id_encuesta = @id_encuesta;
END
GO


CREATE PROCEDURE agregarRespuesta
	@respuesta VARCHAR(255),
	@id_campo INT
AS
BEGIN
	INSERT INTO respuestas
	VALUES(@respuesta, @id_campo);
END
GO

CREATE PROCEDURE obtenerRespuestas
	@id_encuesta INT 
AS
BEGIN
	SELECT c.nombre, r.respuesta
	FROM respuestas r
	INNER JOIN campos c ON c.id_campo = r.id_campo
	INNER JOIN encuestas e ON e.id_encuesta = c.id_encuesta
	WHERE e.id_encuesta = @id_encuesta;
END
GO

CREATE PROCEDURE buscarEncuesta
	@id_encuesta INT 
AS
BEGIN
	SELECT e.id_encuesta
	FROM encuestas e
	WHERE e.id_encuesta = @id_encuesta;
END
GO

CREATE PROCEDURE obtenerIDCampo
	@id_encuesta INT,
	@nombre VARCHAR(255)
AS
BEGIN
	SELECT id_campo
	FROM campos c
	INNER JOIN encuestas e ON e.id_encuesta = c.id_encuesta
	WHERE e.id_encuesta = @id_encuesta
	AND c.nombre = @nombre;
END
GO

