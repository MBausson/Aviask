import { jwtDecode } from "jwt-decode";

//  Returns the URL parameters as a string from a list of JS records
export function getQueryParams(data: Record<string, unknown>): string {
    //  Removes falsy values from data, avoiding to send them in the url & have a 400
    const truthyData = Object.entries(data).filter(([, value]) => {
        if (value instanceof Array) return value.length > 0;

        return value;
    });

    //  Convert data to tuples,
    const paramValueCouples: [string, string][] = [];

    truthyData.forEach((value: [string, unknown]) => {
        if (value[1] instanceof Set) value[1] = Array.from(value[1]);

        //  Breaks down arrays
        //  https://www.linkedin.com/pulse/how-migrate-from-querystring-urlsearchparams-nodejs-vladim%C3%ADr-gorej/
        if (value[1] instanceof Array) {
            value[1].map(v => {
                paramValueCouples.push([value[0], v]);
            });

            return;
        }

        paramValueCouples.push([value[0], value[1] as string]);
    });

    return new URLSearchParams(paramValueCouples).toString();
}

export function timeSince(date: Date): string {
    const now = new Date();
    const diff = Math.floor((now.getTime() - date.getTime()) / 1000);

    const intervals: [number, string][] = [
        [31536000, "year"],
        [2592000, "month"],
        [86400, "day"],
        [3600, "hour"],
        [60, "minute"],
        [1, "second"],
    ];

    for (let i = 0; i < intervals.length; i++) {
        const [intervalInSeconds, unit] = intervals[i];
        const value = Math.floor(diff / intervalInSeconds);
        if (value >= 1) {
            const plural = value !== 1 ? "s" : "";
            return `${value} ${unit}${plural} ago`;
        }
    }

    return "just now";
}

export function shuffleArray<T>(array: T[]): T[] {
    const shuffledArray = [...array];

    for (let i = shuffledArray.length - 1; i > 0; i--) {
        const j = Math.floor(Math.random() * (i + 1));

        [shuffledArray[i], shuffledArray[j]] = [shuffledArray[j], shuffledArray[i]];
    }

    return shuffledArray;
}

export function isJwtExpired(jwt: string): boolean {
    try {
        const decoded = jwtDecode(jwt);

        return (decoded.exp || 0) <= Date.now() / 1000;
    } catch (e) {
        return true;
    }
}
