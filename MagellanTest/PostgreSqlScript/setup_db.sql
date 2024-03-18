create database part; 

\c part

create table item(
    id serial primary key,
    item_name varchar(50) not null,
    parent_item int,
    cost int not null,
    req_date date not null,
    constraint fk_item
            foreign key(parent_item)
                references item(id)
                on delete set null
);

insert into item(item_name, parent_item, cost, req_date)
values('Item1', null, 500, '02-20-2024'),
('Sub1', 1, 200, '02-10-2024'),
('Sub2', 1, 300, '01-05-2024'),
('Sub3', 2, 300, '01-02-2024'),
('Sub4', 2, 400, '01-02-2024'),
('Item2', null, 600, '03-15-2024'),
('Sub1', 6, 200, '02-25-2024');

create function get_total_cost(name varchar(50))
returns int
language plpgsql
as
$$
    declare
        subs int[];
        newsubs int[];
        iviews record;
        total int := 0;
        x int;
    begin
        select id from item 
        into iviews
        where item_name = name and parent_item is null;

        if not found then
            return null; 
        end if; 
  
        subs := subs || iviews.id;
        while array_length(subs, 1) is not null loop
            newsubs := array[]::int[]; 
            foreach x in array subs loop
                select cost from item into iviews where id = x; 
                total := total + iviews.cost; 
                for iviews in select id from item where parent_item = x loop
                    newsubs := newsubs || iviews.id; 
                end loop; 
            end loop;
        subs := newsubs;  
        end loop;
  
        return total; 
    end; 
$$;