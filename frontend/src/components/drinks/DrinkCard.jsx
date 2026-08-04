function DrinkCard({ title, description }) {
  return (
    <article className="mt-6 rounded-2xl border border-border bg-card p-6 shadow-lg shadow-black/20">
      <h2 className="text-xl font-semibold text-white">{title}</h2>
      <p className="mt-3 text-sm leading-6 text-slate-400">{description}</p>
    </article>
  );
}

export default DrinkCard;
