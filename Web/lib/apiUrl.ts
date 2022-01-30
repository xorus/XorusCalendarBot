export function apiUrl(url: string,
                       params?: { [key: string]: string | undefined },
                       replace: { [key: string]: string } = {}): string {
    url = (process.env.NEXT_PUBLIC_BASE_API_URL ?? "") + url;

    if (replace && Object.entries(replace).length > 0) {
        for (const key in replace) {
            url = url.replace(`{${key}}`, encodeURIComponent(replace[key]));
        }
    }

    if (params && Object.entries(params).length > 0) {
        let i = 0;
        for (const key in params) {
            if (typeof params[key] === "undefined") {
                continue;
            }

            url += i++ === 0 ? "?" : "&";
            url += encodeURIComponent(key) + "=" + encodeURIComponent(params[key] as string);
        }
    }

    return url;
}
