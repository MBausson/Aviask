import type { AviaskUserDetails } from "./DTO/responseModels";

export type UserRoleValue = "member" | "manager" | "admin";

export class UserRole {
    public name: string;
    public value: UserRoleValue;
    public hierarchyValue: number;

    private constructor(name: string, value: UserRoleValue, hierarchyValue: number) {
        this.name = name;
        this.value = value;
        this.hierarchyValue = hierarchyValue;
    }

    static roles: UserRole[] = [
        new UserRole("Member", "member", 1),
        new UserRole("Manager", "manager", 2),
        new UserRole("Admin", "admin", 3),
    ];

    static getUserRole(value: UserRoleValue): UserRole {
        return this.roles.filter(r => r.value == value).at(0) || this.roles[0];
    }
}

export class AviaskUser {
    public id: string;
    public username: string;
    public role: UserRole;
    public createdAt: Date;
    public isPremium: boolean;

    constructor(details: AviaskUserDetails) {
        this.id = details.id;
        this.username = details.userName;
        this.role = UserRole.getUserRole(details.role as UserRoleValue);
        this.createdAt = new Date(details.createdAt);
        this.isPremium = details.isPremium;
    }

    toInterface(): AviaskUserDetails {
        return {
            id: this.id,
            role: this.role.name,
            createdAt: this.createdAt.toLocaleString(),
            isPremium: this.isPremium,
            userName: this.username,
        };
    }
}
