export function buildUrl(input, options) {
    const queryString = [];
    let builtUrl;
    const caseChange = !!(options && !!options.lowerCase);

    if (input === null) {
        builtUrl = '';
    } else if (typeof (input) === 'object') {
        builtUrl = '';
        options = input;
    } else {
        builtUrl = input;
    }

    if (builtUrl && builtUrl[builtUrl.length - 1] === '/') {
        builtUrl = builtUrl.slice(0, -1);
    }

    if (options) {
        if (options.path) {
            let localVar = String(options.path).trim();
            if (caseChange) {
                localVar = localVar.toLowerCase();
            }
            if (localVar.indexOf('/') === 0) {
                builtUrl += localVar;
            } else {
                builtUrl += '/' + localVar;
            }
        }

        if (options.queryParams) {
            for (const key in options.queryParams) {
                if (options.queryParams.hasOwnProperty(key) && options.queryParams[key] !== void 0) {
                    let encodedParam;
                    if (caseChange) {
                        encodedParam = encodeURIComponent(String(options.queryParams[key]).trim().toLowerCase());
                    }
                    else {
                        encodedParam = encodeURIComponent(String(options.queryParams[key]).trim());
                    }
                    queryString.push(key + '=' + encodedParam);
                }
            }
            builtUrl += '?' + queryString.join('&');
        }

        if (options.hash) {
            if (caseChange) {
                builtUrl += '#' + String(options.hash).trim().toLowerCase();
            }
            else {
                builtUrl += '#' + String(options.hash).trim();
            }
        }
    }
    return builtUrl;
};
