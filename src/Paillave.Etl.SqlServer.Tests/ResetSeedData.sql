
alter table People set (system_versioning = off);
truncate table People;
alter table People set (system_versioning = on (history_table = dbo.PeopleHistory));

truncate table Groups;

insert into People (Firstname, Lastname, LastnamePrefix, [Role])
values ('Thomas', 'Bangalter', null, null),
       ('Guy-Manuel', 'de Homem-Christo', null, null),
       ('Nicolas', 'Godin', null, 'Sexy boy'),
       ('Jean-Benoît', 'Dunckel', null, 'Sexy boy');