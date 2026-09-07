import { z } from "zod";

export const contactSchema = z.object({
  name: z
    .string()
    .min(2, { message: "이름은 2자 이상 입력해 주세요." })
    .max(40, { message: "이름은 40자 이하로 입력해 주세요." }),
  email: z.string().email({ message: "올바른 이메일 주소를 입력해 주세요." }),
  message: z
    .string()
    .min(10, { message: "문의 내용은 10자 이상 입력해 주세요." })
    .max(1000, { message: "문의 내용은 1000자 이하로 입력해 주세요." }),
});

export type ContactInput = z.infer<typeof contactSchema>;
