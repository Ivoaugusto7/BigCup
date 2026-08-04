function StatusBadge({ label, tone = 'info' }) {
  const tones = {
    info: 'bg-emerald-500/10 text-emerald-300 ring-emerald-500/30',
    danger: 'bg-red-500/10 text-red-300 ring-red-500/30',
  };

  return (
    <span className={`mt-4 inline-flex items-center rounded-full px-3 py-1 text-sm font-medium ring-1 ${tones[tone]}`}>
      {label}
    </span>
  );
}

export default StatusBadge;
