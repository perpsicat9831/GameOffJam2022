{{~
    name = x.name
    namespace = x.namespace
    tables = x.tables
~}}

package {{namespace}}

import "bright/serialization"

type ByteBufLoader func(string) (*serialization.ByteBuf, error)

type {{name}} struct {
    {{~for table in tables ~}}
    {{table.name}} *{{table.go_full_name}}
    {{~end~}}
}

func NewTables(loader ByteBufLoader) (*{{name}}, error) {
    var err error
    var buf *serialization.ByteBuf

    tables := &{{name}}{}
    {{~for table in tables ~}}
    if buf, err = loader("{{table.output_data_file}}") ; err != nil {
        return nil, err
    }
    if tables.{{table.name}}, err = New{{table.go_full_name}}(buf) ; err != nil {
        return nil, err
    }
    {{~end~}}
    return tables, nil
}
