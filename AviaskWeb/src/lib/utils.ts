import { type ClassValue, clsx } from "clsx";
import { twMerge } from "tailwind-merge";

export function cn(...inputs: ClassValue[]) {
    return twMerge(clsx(inputs));
}

export function IsFeatureEnabled(featureName: string): boolean {
    return import.meta.env[`FEATURE_${featureName.toUpperCase()}`] == "true";
}
