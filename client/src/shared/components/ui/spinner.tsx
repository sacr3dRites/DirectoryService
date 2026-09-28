import { LoaderCircle } from "lucide-react";

type SpinnerProps = React.ComponentProps<typeof LoaderCircle>;

function Spinner({ className, ...props }: SpinnerProps) {
  return (
    <LoaderCircle
      role="status"
      aria-label="Загрузка"
      className={`size-5 animate-spin ${className ?? ""}`}
      {...props}
    />
  );
}

export { Spinner };
