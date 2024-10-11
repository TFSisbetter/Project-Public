import { User } from "../user";
import { State } from "../state";

export interface ReadKnownUserResponse {
    user: User | null;
    success: boolean;
    errorAuthentication: boolean;
    errorItemNotFound: boolean;
    state: State | null;
}