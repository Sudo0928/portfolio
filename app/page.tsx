import { About } from "@/components/sections/about";
import { AiWorkflow } from "@/components/sections/ai-workflow";
import { Contact } from "@/components/sections/contact";
import { Experience } from "@/components/sections/experience";
import { Hero } from "@/components/sections/hero";
import { Projects } from "@/components/sections/projects";
import { TechStack } from "@/components/sections/tech-stack";

export default function Home() {
  return (
    <>
      <Hero />
      <About />
      <Projects />
      <TechStack />
      <AiWorkflow />
      <Experience />
      <Contact />
    </>
  );
}
