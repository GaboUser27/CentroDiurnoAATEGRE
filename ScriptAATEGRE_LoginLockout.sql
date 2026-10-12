/*
  Ejecutar en la base local de AATEGRE (la misma que usa appsettings.Development.json).
  Agrega las columnas usadas por el bloqueo de inicio de sesión sin modificar
  usuarios, roles ni contraseñas. Es seguro volver a ejecutar.
*/

IF COL_LENGTH(N'dbo.Usuario', N'IntentosFallidos') IS NULL
BEGIN
    ALTER TABLE dbo.Usuario
        ADD IntentosFallidos int NOT NULL
            CONSTRAINT DF_Usuario_IntentosFallidos DEFAULT (0) WITH VALUES;
END;

IF COL_LENGTH(N'dbo.Usuario', N'BloqueadoHasta') IS NULL
BEGIN
    ALTER TABLE dbo.Usuario
        ADD BloqueadoHasta datetime2 NULL;
END;
