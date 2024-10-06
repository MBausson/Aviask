import ModalComponent from "../Widgets/ModalComponent.vue";
import { it, expect } from "vitest";

it("testing ModalComponent component props", async () => {
    //  modelValue
    expect(ModalComponent.props.show).toBeTruthy();
    expect(ModalComponent.props.show.type()).toBeTypeOf("boolean");
    expect(ModalComponent.props.show.required).true;
});
