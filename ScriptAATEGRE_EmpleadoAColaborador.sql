/*
  Normaliza el nombre del rol en la base AATEGRE.
  Si ya existe Colaborador, reasigna sus usuarios y elimina el rol duplicado.
  No cambia contraseñas ni otros datos de usuario.
*/
SET XACT_ABORT ON;

BEGIN TRY
    BEGIN TRANSACTION;

    DECLARE @EmpleadoId int = (SELECT TOP (1) IdRol FROM dbo.Rol WHERE Nombre = 'Empleado');
    DECLARE @ColaboradorId int = (SELECT TOP (1) IdRol FROM dbo.Rol WHERE Nombre = 'Colaborador');

    IF @EmpleadoId IS NOT NULL
    BEGIN
        IF @ColaboradorId IS NULL
        BEGIN
            UPDATE dbo.Rol
            SET Nombre = 'Colaborador'
            WHERE IdRol = @EmpleadoId;
        END
        ELSE
        BEGIN
            UPDATE dbo.Usuario
            SET IdRol = @ColaboradorId
            WHERE IdRol = @EmpleadoId;

            DELETE FROM dbo.Rol
            WHERE IdRol = @EmpleadoId;
        END;
    END;

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
