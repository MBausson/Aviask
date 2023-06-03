select UserName, Name RoleName from AspNetRoles r, AspNetUserRoles ur, AspNetUsers u
where u.Id = ur.UserId
and r.Id = ur.RoleId;

select r.Id from AspNetRoles r where Name = 'admin';

update AspNetUserRoles
set RoleId = (select r.Id from AspNetRoles r where r.Name = 'admin');