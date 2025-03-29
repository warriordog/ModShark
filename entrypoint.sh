#!/bin/bash
if [[ $# > 0 ]]; then
    tmpfile=$(mktemp)
    vars=()
    while IFS='=' read -r -d '' n v; do
        if [[ $n == "MODSHARK__"* ]]; then
            vars+=("$(printf "%s=%s" "$n" "$v")")
        fi
    done < <(env -0)

    for var in "${vars[@]}"; do 
        item=$(echo "${var}" | grep  ^MODSHARK__ | sed s/^MODSHARK__/./g | sed s/__/./g)
        key=$(echo "${item}" | cut -d "=" -f 1)
        value=$(echo "${item}" | cut -d "=" -f 2-)
        jq $key="\"$value\"" appsettings.json > $tmpfile && \
        mv $tmpfile appsettings.json
    done
    exec $@
else
    /bin/bash
fi
